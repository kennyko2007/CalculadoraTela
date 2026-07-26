using CalculadoraTela.Data;
using CalculadoraTela.Models;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraTela.Controllers;

/// <summary>
/// Panel de administración: solo accesible para usuarios en el rol "Admin".
/// Es el único lugar de la app donde se crean, editan, cambian de
/// contraseña, suspenden o eliminan cuentas. No depende de correo
/// electrónico para nada de esto.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private const string RolAdmin = "Admin";

    // Fecha "muy lejana" usada como marca de suspensión indefinida
    // (distinta de un bloqueo temporal normal por intentos fallidos).
    private static readonly DateTimeOffset FechaSuspensionIndefinida = DateTimeOffset.UtcNow.AddYears(100);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context, ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    private static bool EstaSuspendido(ApplicationUser usuario) =>
        usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1);

    // ───────────────────────────── LISTADO ─────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios = await _userManager.Users
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var lista = new List<AdminUserListItemViewModel>();
        foreach (var usuario in usuarios)
        {
            lista.Add(new AdminUserListItemViewModel
            {
                Id = usuario.Id,
                UserName = usuario.UserName ?? "",
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email ?? "",
                FechaRegistro = usuario.FechaRegistro,
                EsAdministrador = await _userManager.IsInRoleAsync(usuario, RolAdmin),
                Suspendido = EstaSuspendido(usuario)
            });
        }

        return View(lista);
    }

    // ───────────────────────────── CREAR ─────────────────────────────

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AdminCreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCreateUserViewModel model)
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
            FechaRegistro = DateTime.UtcNow,
            EmailConfirmed = true
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

        if (model.EsAdministrador)
        {
            await _userManager.AddToRoleAsync(nuevoUsuario, RolAdmin);
        }

        _logger.LogInformation("El administrador {Admin} creó el usuario {Usuario}.", User.Identity!.Name, nuevoUsuario.UserName);

        TempData["Mensaje"] = $"Usuario '{nuevoUsuario.UserName}' creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ───────────────────────────── EDITAR ─────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        var model = new AdminEditUserViewModel
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            UserName = usuario.UserName ?? "",
            Email = usuario.Email ?? "",
            EsAdministrador = await _userManager.IsInRoleAsync(usuario, RolAdmin)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminEditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        // Verificar que el nuevo usuario/correo no choquen con OTRA cuenta.
        var usuarioConEseNombre = await _userManager.FindByNameAsync(model.UserName);
        if (usuarioConEseNombre != null && usuarioConEseNombre.Id != usuario.Id)
        {
            ModelState.AddModelError(nameof(model.UserName), "Ese nombre de usuario ya está en uso.");
            return View(model);
        }

        var usuarioConEseCorreo = await _userManager.FindByEmailAsync(model.Email);
        if (usuarioConEseCorreo != null && usuarioConEseCorreo.Id != usuario.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "Ese correo ya está registrado.");
            return View(model);
        }

        usuario.NombreCompleto = model.NombreCompleto;

        if (usuario.UserName != model.UserName)
        {
            await _userManager.SetUserNameAsync(usuario, model.UserName);
        }
        if (usuario.Email != model.Email)
        {
            await _userManager.SetEmailAsync(usuario, model.Email);
            usuario.EmailConfirmed = true;
        }

        var resultado = await _userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // No permitir quitarse el rol de admin a uno mismo si es el único admin.
        var esAdminActualmente = await _userManager.IsInRoleAsync(usuario, RolAdmin);
        if (esAdminActualmente && !model.EsAdministrador)
        {
            var admins = await _userManager.GetUsersInRoleAsync(RolAdmin);
            if (admins.Count <= 1)
            {
                TempData["Error"] = "No puedes quitarle el rol de administrador al único administrador que queda.";
                return RedirectToAction(nameof(Index));
            }
            await _userManager.RemoveFromRoleAsync(usuario, RolAdmin);
        }
        else if (!esAdminActualmente && model.EsAdministrador)
        {
            await _userManager.AddToRoleAsync(usuario, RolAdmin);
        }

        _logger.LogInformation("El administrador {Admin} editó al usuario {Usuario}.", User.Identity!.Name, usuario.UserName);

        TempData["Mensaje"] = $"Usuario '{usuario.UserName}' actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ───────────────────────── CAMBIAR CONTRASEÑA ─────────────────────────

    [HttpGet]
    public async Task<IActionResult> ChangePassword(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(new AdminChangePasswordViewModel { Id = usuario.Id, UserName = usuario.UserName ?? "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        // El admin fija la contraseña directamente, sin token por correo:
        // se genera y se consume el token de reseteo en el mismo instante.
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, model.NewPassword);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.UserName = usuario.UserName ?? "";
            return View(model);
        }

        _logger.LogInformation("El administrador {Admin} cambió la contraseña de {Usuario}.", User.Identity!.Name, usuario.UserName);

        TempData["Mensaje"] = $"Contraseña de '{usuario.UserName}' actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ───────────────────────── SUSPENDER / REACTIVAR ─────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspender(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        if (usuario.UserName == User.Identity!.Name)
        {
            TempData["Error"] = "No puedes suspender tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(usuario, RolAdmin))
        {
            var admins = await _userManager.GetUsersInRoleAsync(RolAdmin);
            if (admins.Count <= 1)
            {
                TempData["Error"] = "No puedes suspender al único administrador que queda.";
                return RedirectToAction(nameof(Index));
            }
        }

        await _userManager.SetLockoutEnabledAsync(usuario, true);
        await _userManager.SetLockoutEndDateAsync(usuario, FechaSuspensionIndefinida);

        _logger.LogInformation("El administrador {Admin} suspendió al usuario {Usuario}.", User.Identity!.Name, usuario.UserName);

        TempData["Mensaje"] = $"Usuario '{usuario.UserName}' suspendido. Ya no puede iniciar sesión.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.SetLockoutEndDateAsync(usuario, null);

        _logger.LogInformation("El administrador {Admin} reactivó al usuario {Usuario}.", User.Identity!.Name, usuario.UserName);

        TempData["Mensaje"] = $"Usuario '{usuario.UserName}' reactivado. Ya puede iniciar sesión de nuevo.";
        return RedirectToAction(nameof(Index));
    }

    // ───────────────────────────── ELIMINAR ─────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        if (usuario.UserName == User.Identity!.Name)
        {
            TempData["Error"] = "No puedes eliminar tu propia cuenta mientras tienes la sesión iniciada.";
            return RedirectToAction(nameof(Index));
        }

        // Evita quedarse sin ningún administrador en el sistema.
        if (await _userManager.IsInRoleAsync(usuario, RolAdmin))
        {
            var admins = await _userManager.GetUsersInRoleAsync(RolAdmin);
            if (admins.Count <= 1)
            {
                TempData["Error"] = "No puedes eliminar al único administrador que queda.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Sus cálculos guardados quedan sin dueño (no se borran).
        var resultado = await _userManager.DeleteAsync(usuario);

        if (resultado.Succeeded)
        {
            _logger.LogInformation("El administrador {Admin} eliminó al usuario {Usuario}.", User.Identity!.Name, usuario.UserName);
            TempData["Mensaje"] = $"Usuario '{usuario.UserName}' eliminado.";
        }
        else
        {
            TempData["Error"] = "No se pudo eliminar el usuario: " +
                string.Join(", ", resultado.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Cierra la sesión activa del usuario sin eliminar su cuenta ni
    /// suspenderla, cambiando su "security stamp". Identity revalida ese
    /// valor contra la base cada ~30 minutos, así que su sesión actual se
    /// corta, pero puede volver a iniciar sesión de inmediato si quiere.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForzarCierreSesion(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.UpdateSecurityStampAsync(usuario);

        TempData["Mensaje"] = $"Se cerró la sesión activa de '{usuario.UserName}'.";
        return RedirectToAction(nameof(Index));
    }
}
