using CalculadoraTela.Models;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraTela.Controllers;

/// <summary>
/// Cuentas de usuario. El registro público, la recuperación de contraseña
/// por correo y el cambio de contraseña autoservicio se quitaron a propósito:
/// ahora solo el administrador (panel /Admin) puede crear usuarios, cambiar
/// sus contraseñas, editarlos, suspenderlos o eliminarlos. No se depende
/// de ningún correo electrónico para gestionar cuentas.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        if (resultado.IsLockedOut)
        {
            // Distinguimos "suspendido por el administrador" (bloqueo muy
            // largo) de un bloqueo temporal normal por intentos fallidos.
            var esSuspensionIndefinida = usuario.LockoutEnd.HasValue &&
                usuario.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1);

            ModelState.AddModelError(string.Empty, esSuspensionIndefinida
                ? "Esta cuenta fue suspendida por el administrador."
                : "Cuenta bloqueada temporalmente por demasiados intentos fallidos. Intenta de nuevo en unos minutos.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        }

        return View(model);
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
}
