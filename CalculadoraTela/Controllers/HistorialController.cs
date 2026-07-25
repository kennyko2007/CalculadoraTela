using CalculadoraTela.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CalculadoraTela.Controllers;

[Authorize]
public class HistorialController : Controller
{
    private readonly AppDbContext _context;

    public HistorialController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _context.Calculos.AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            query = query.Where(c => c.UserId == userId);
        }

        var historial = await query
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
        return View(historial);
    }
}