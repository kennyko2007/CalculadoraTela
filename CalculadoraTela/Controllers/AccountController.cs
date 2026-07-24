using System.Text;
using System.Text.Encodings.Web;
using CalculadoraTela.Models;
using CalculadoraTela.Services;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CalculadoraTela.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    // ───────────────────────────── LOGIN ─────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Login por NOMBRE DE USUARIO (no por correo).
        var usuario = await _userManager.FindByNameAsync(model.UserName);

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            usuario.UserName!,
            model.Password,
            isPersistent: model.RecordarSesion,
            lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            _logger.LogInformation("El usuario {Usuario} inició sesión.", usuario.UserName);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        if (resultado.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty,
                "Debes confirmar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada.");
        }
        else if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Cuenta bloqueada temporalmente por demasiados intentos fallidos. Intenta de nuevo en unos minutos.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        }

        return View(model);
    }

    // ──────────────────────────── REGISTRO ────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuarioExistente = await _userManager.FindByNameAsync(model.UserName);
        if (usuarioExistente != null)
        {
            ModelState.AddModelError(nameof(model.UserName), "Ese nombre de usuario ya está en uso.");
            return View(model);
        }

        var correoExistente = await _userManager.FindByEmailAsync(model.Email);
        if (correoExistente != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Ese correo ya está registrado.");
            return View(model);
        }

        var nuevoUsuario = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            NombreCompleto = model.NombreCompleto,
            FechaRegistro = DateTime.UtcNow
        };

        var resultado = await _userManager.CreateAsync(nuevoUsuario, model.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // Generar el enlace de confirmación y enviarlo por Gmail.
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(nuevoUsuario);
        var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var enlaceConfirmacion = Url.Action(
            "ConfirmEmail",
            "Account",
            new { userId = nuevoUsuario.Id, token = tokenCodificado },
            protocol: Request.Scheme);

        try
        {
            await _emailSender.SendEmailAsync(
                nuevoUsuario.Email,
                "Confirma tu cuenta - Calculadora de Tela",
                ConstruirCorreoConfirmacion(nuevoUsuario.NombreCompleto, enlaceConfirmacion!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo de confirmación a {Email}.", nuevoUsuario.Email);
            TempData["EmailError"] =
                "Tu cuenta se creó, pero no pudimos enviarte el correo de confirmación. Contacta al administrador.";
        }

        return RedirectToAction(nameof(RegisterConfirmation), new { email = nuevoUsuario.Email });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult RegisterConfirmation(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    // ───────────────────────── CONFIRMAR CORREO ─────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null)
        {
            ViewBag.Exito = false;
            return View();
        }

        string tokenDecodificado;
        try
        {
            tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch
        {
            ViewBag.Exito = false;
            return View();
        }

        var resultado = await _userManager.ConfirmEmailAsync(usuario, tokenDecodificado);
        ViewBag.Exito = resultado.Succeeded;

        return View();
    }

    // ─────────────────────── OLVIDÉ MI CONTRASEÑA ───────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _userManager.FindByEmailAsync(model.Email);

        // Por seguridad, siempre mostramos el mismo mensaje exista o no la
        // cuenta, para no revelar qué correos están registrados.
        if (usuario != null && await _userManager.IsEmailConfirmedAsync(usuario))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var enlaceReset = Url.Action(
                "ResetPassword",
                "Account",
                new { email = usuario.Email, token = tokenCodificado },
                protocol: Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(
                    usuario.Email!,
                    "Recupera tu contraseña - Calculadora de Tela",
                    ConstruirCorreoRecuperacion(usuario.NombreCompleto, enlaceReset!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de recuperación a {Email}.", usuario.Email);
            }
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    // ─────────────────────── RESTABLECER CONTRASEÑA ───────────────────────

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _userManager.FindByEmailAsync(model.Email);
        if (usuario == null)
        {
            // No revelamos si el correo existe o no.
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        string tokenDecodificado;
        try
        {
            tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "El enlace de recuperación no es válido o ya expiró.");
            return View(model);
        }

        var resultado = await _userManager.ResetPasswordAsync(usuario, tokenDecodificado, model.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    // ─────────────────────── CAMBIAR CONTRASEÑA (logueado) ───────────────────────

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var resultado = await _userManager.ChangePasswordAsync(usuario, model.CurrentPassword, model.NewPassword);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // Refresca la cookie de sesión para que no se cierre tras cambiar la contraseña.
        await _signInManager.RefreshSignInAsync(usuario);

        TempData["Mensaje"] = "Tu contraseña se actualizó correctamente.";
        return RedirectToAction(nameof(ChangePassword));
    }

    // ───────────────────────────── LOGOUT ─────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ───────────────────────────── HELPERS ─────────────────────────────

    private static string ConstruirCorreoRecuperacion(string nombre, string enlace)
    {
        var enlaceSeguro = HtmlEncoder.Default.Encode(enlace);
        return $@"
            <div style='font-family:Segoe UI,Arial,sans-serif; max-width:480px; margin:auto; background:#f4f6fb; padding:32px; border-radius:12px;'>
                <div style='background:linear-gradient(135deg,#060f1e,#0f2040); padding:20px 24px; border-radius:10px 10px 0 0;'>
                    <h2 style='color:#ffffff; margin:0; font-size:20px;'>Calculadora de Tela</h2>
                </div>
                <div style='background:#ffffff; padding:28px 24px; border-radius:0 0 10px 10px; border:1px solid #e2e8f0; border-top:none;'>
                    <p style='color:#0b1629; font-size:15px;'>Hola <strong>{nombre}</strong>,</p>
                    <p style='color:#334155; font-size:14px; line-height:1.5;'>
                        Recibimos una solicitud para restablecer tu contraseña. Haz clic en el siguiente botón para elegir una nueva:
                    </p>
                    <div style='text-align:center; margin:28px 0;'>
                        <a href='{enlaceSeguro}'
                           style='background:#2563eb; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:8px; font-weight:600; font-size:14px; display:inline-block;'>
                            Restablecer contraseña
                        </a>
                    </div>
                    <p style='color:#64748b; font-size:12px;'>
                        Si el botón no funciona, copia y pega este enlace en tu navegador:<br />
                        <a href='{enlaceSeguro}' style='color:#2563eb; word-break:break-all;'>{enlaceSeguro}</a>
                    </p>
                    <p style='color:#94a3b8; font-size:12px; margin-top:24px;'>
                        Si no solicitaste este cambio, puedes ignorar este correo: tu contraseña seguirá siendo la misma.
                    </p>
                </div>
            </div>";
    }

    private static string ConstruirCorreoConfirmacion(string nombre, string enlace)
    {
        var enlaceSeguro = HtmlEncoder.Default.Encode(enlace);
        return $@"
            <div style='font-family:Segoe UI,Arial,sans-serif; max-width:480px; margin:auto; background:#f4f6fb; padding:32px; border-radius:12px;'>
                <div style='background:linear-gradient(135deg,#060f1e,#0f2040); padding:20px 24px; border-radius:10px 10px 0 0;'>
                    <h2 style='color:#ffffff; margin:0; font-size:20px;'>Calculadora de Tela</h2>
                </div>
                <div style='background:#ffffff; padding:28px 24px; border-radius:0 0 10px 10px; border:1px solid #e2e8f0; border-top:none;'>
                    <p style='color:#0b1629; font-size:15px;'>Hola <strong>{nombre}</strong>,</p>
                    <p style='color:#334155; font-size:14px; line-height:1.5;'>
                        Gracias por registrarte. Confirma tu correo electrónico haciendo clic en el siguiente botón:
                    </p>
                    <div style='text-align:center; margin:28px 0;'>
                        <a href='{enlaceSeguro}'
                           style='background:#2563eb; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:8px; font-weight:600; font-size:14px; display:inline-block;'>
                            Confirmar mi correo
                        </a>
                    </div>
                    <p style='color:#64748b; font-size:12px;'>
                        Si el botón no funciona, copia y pega este enlace en tu navegador:<br />
                        <a href='{enlaceSeguro}' style='color:#2563eb; word-break:break-all;'>{enlaceSeguro}</a>
                    </p>
                    <p style='color:#94a3b8; font-size:12px; margin-top:24px;'>
                        Si no creaste esta cuenta, puedes ignorar este correo.
                    </p>
                </div>
            </div>";
    }
}
