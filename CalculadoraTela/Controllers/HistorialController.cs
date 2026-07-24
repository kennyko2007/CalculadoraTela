using CalculadoraTela.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraTela.Controllers;

public class HistorialController : Controller
{
    private readonly AppDbContext _context;

    public HistorialController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var historial = await _context.Calculos
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
        return View(historial);
    }
}