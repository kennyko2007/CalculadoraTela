using CalculadoraTela.Data;
using CalculadoraTela.Models;
using CalculadoraTela.Services;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraTela.Controllers;

public class HomeController : Controller
{
    private readonly CalculadoraService _calculadoraService;
    private readonly AppDbContext _context;

    public HomeController(CalculadoraService calculadoraService, AppDbContext context)
    {
        _calculadoraService = calculadoraService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new CalculadoraTelaVM();
        model = _calculadoraService.CalcularValores(model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Historial()
    {
        try
        {
            var listaHistorial = await _context.Calculos
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return View(listaHistorial);
        }
        catch (Exception ex)
        {
            return Content($"Error crítico al cargar el historial: {ex.Message} --- Detalle: {ex.InnerException?.Message}");
        }
    }

    [HttpPost]
    public IActionResult Calcular([FromBody] CalculadoraTelaVM model)
    {
        var resultado = _calculadoraService.CalcularValores(model);
        return Json(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarHistorial([FromBody] CalculadoraTelaVM model)
    {
        try
        {
            var calculado = _calculadoraService.CalcularValores(model);

            var entidad = new Calculo
            {
                FechaCreacion = DateTime.UtcNow,
                UrdimbreTejido = calculado.UrdimbreTejido,
                UrdimbreDenier = calculado.UrdimbreDenier,
                TramaTejido = calculado.TramaTejido,
                TramaDenier = calculado.TramaDenier,
                Laminado = calculado.Laminado,
                AnchoRefuerzoFactor = calculado.AnchoRefuerzoFactor,
                Ancho = calculado.Ancho,
                Lado = calculado.Lado,
                Corte = calculado.Corte,
                Costura = calculado.Costura,
                MaquinaNumero = calculado.MaquinaNumero,
                Engranaje = calculado.Engranaje,
                Horas = calculado.Horas,
                ResistenciaUrdimbre = calculado.ResistenciaUrdimbre,
                PesoUrdimbre = calculado.PesoUrdimbre,
                PorcentajeUrdimbre = calculado.PorcentajeUrdimbre,
                ResistenciaTrama = calculado.ResistenciaTrama,
                PesoTrama = calculado.PesoTrama,
                PorcentajeTrama = calculado.PorcentajeTrama,
                PesoTejidoBase = calculado.PesoTejidoBase,
                PesoConLaminado = calculado.PesoConLaminado,
                PesoConRefuerzo = calculado.PesoConRefuerzo,
                PesoMetroLineal = calculado.PesoMetroLineal,
                PesoPorBolsa = calculado.PesoPorBolsa,
                ProduccionEstimada = calculado.ProduccionEstimada,
                ResumenFicha = $"{calculado.Ancho:0.0}x{calculado.Lado:0.0} | {calculado.UrdimbreTejido:0.0}x{calculado.TramaTejido:0.0}"
            };

            _context.Calculos.Add(entidad);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = entidad.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                success = false, 
                message = ex.Message, 
                inner = ex.InnerException?.Message 
            });
        }
    }

    // --- MÉTODOS PARA ELIMINAR ---

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var registro = await _context.Calculos.FindAsync(id);
        if (registro != null)
        {
            _context.Calculos.Remove(registro);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Registro no encontrado." });
    }

    [HttpPost]
    public async Task<IActionResult> EliminarSeleccionados([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return Json(new { success = false, message = "No se seleccionó ningún registro." });
        }

        var registros = await _context.Calculos.Where(c => ids.Contains(c.Id)).ToListAsync();
        _context.Calculos.RemoveRange(registros);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}
