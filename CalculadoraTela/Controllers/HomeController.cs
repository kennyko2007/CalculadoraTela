using CalculadoraTela.Data;
using CalculadoraTela.Models;
using CalculadoraTela.Services;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public IActionResult Calcular(CalculadoraTelaVM model)
    {
        var resultado = _calculadoraService.CalcularValores(model);
        return Json(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarHistorial([FromBody] CalculadoraTelaVM model)
    {
        var calculado = _calculadoraService.CalcularValores(model);

        var entidad = new Calculo
        {
            FechaCreacion = DateTime.Now,
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
            ResumenFicha = calculado.ResumenFicha
        };

        _context.Calculos.Add(entidad);
        await _context.SaveChangesAsync();

        return Json(new { success = true, id = entidad.Id });
    }
}