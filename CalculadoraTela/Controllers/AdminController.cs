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
/// Permite crear nuevos usuarios (sin pasar por verificación de correo,
/// ya que el administrador define la contraseña directamente) y eliminarlos.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private const string RolAdmin = "Admin";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context, ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

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
                EsAdministrador = await _userManager.IsInRoleAsync(usuario, RolAdmin)
            });
        }

        return View(lista);
    }

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
            // Creado por el administrador: no necesita verificar su correo.
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
    /// Cierra la sesión activa del usuario sin eliminar su cuenta, cambiando
    /// su "security stamp". Identity revalida ese valor contra la base cada
    /// ~30 minutos, así que su sesión actual se corta sin borrar sus datos.
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
