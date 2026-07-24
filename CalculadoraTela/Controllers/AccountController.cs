using CalculadoraTela.Models;
using CalculadoraTela.Services;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraTela.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly EmailSender _emailSender;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        EmailSender emailSender,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    // ── REGISTRO ──────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            NombreCompleto = model.NombreCompleto
        };

        var resultado = await _userManager.CreateAsync(usuario, model.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Generar token de confirmación y enviar correo
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
        var callbackUrl = Url.Action(
            "ConfirmarEmail", "Account",
            new { userId = usuario.Id, token },
            Request.Scheme)!;

        var cuerpo = $@"
            <div style='font-family:sans-serif;max-width:520px;margin:auto;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden;'>
              <div style='background:linear-gradient(135deg,#060f1e,#0f2040);padding:28px 32px;'>
                <h2 style='color:#fff;margin:0;font-size:1.3rem;'>Calculadora Tela — Verifica tu correo</h2>
              </div>
              <div style='padding:28px 32px;background:#fff;'>
                <p style='color:#1e293b;'>Hola <strong>{model.NombreCompleto}</strong>,</p>
                <p style='color:#475569;'>Haz clic en el botón para activar tu cuenta:</p>
                <a href='{callbackUrl}'
                   style='display:inline-block;margin:16px 0;padding:12px 28px;background:#2563eb;color:#fff;
                          text-decoration:none;border-radius:8px;font-weight:600;'>
                  Confirmar cuenta
                </a>
                <p style='color:#94a3b8;font-size:0.82rem;'>Si no creaste esta cuenta, ignora este mensaje.</p>
              </div>
            </div>";

        try
        {
            await _emailSender.EnviarAsync(model.Email, "Confirma tu cuenta — Calculadora Tela", cuerpo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo de confirmación a {Email}.", model.Email);
            ModelState.AddModelError(string.Empty, "No se pudo enviar el correo de verificación. Revisa la configuración SMTP.");
            await _userManager.DeleteAsync(usuario);
            return View(model);
        }

        return RedirectToAction("RegistroExitoso");
    }

    [AllowAnonymous]
    public IActionResult RegistroExitoso() => View();

    // ── CONFIRMACIÓN DE EMAIL ─────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return RedirectToAction("Login");

        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null) return NotFound();

        var resultado = await _userManager.ConfirmEmailAsync(usuario, token);
        ViewBag.Exito = resultado.Succeeded;
        return View();
    }

    // ── LOGIN ─────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var usuario = await _userManager.FindByEmailAsync(model.Email);
        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        if (!await _userManager.IsEmailConfirmedAsync(usuario))
        {
            ModelState.AddModelError(string.Empty, "Debes confirmar tu correo antes de iniciar sesión.");
            return View(model);
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            usuario, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            _logger.LogInformation("Usuario {Email} inició sesión.", model.Email);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente por demasiados intentos fallidos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(model);
    }

    // ── LOGOUT ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
