using CalculadoraTela.Data;
using CalculadoraTela.Models;
using CalculadoraTela.Services;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraTela.Controllers
{
    public class HomeController : Controller
    {
        private readonly CalculadoraService _calculadoraService;
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CalculadoraService calculadoraService, AppDbContext context, ILogger<HomeController> logger)
        {
            _calculadoraService = calculadoraService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new CalculadoraTelaVM();
            try
            {
                if (_calculadoraService != null)
                {
                    model = _calculadoraService.Calcular(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular valores iniciales en Index.");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult CalcularAjax([FromBody] CalculadoraTelaVM model)
        {
            if (model == null) model = new CalculadoraTelaVM();
            
            var resultado = _calculadoraService.Calcular(model);
            return Json(new { success = true, data = resultado });
        }

        [HttpGet]
        public async Task<IActionResult> Historial()
        {
            try
            {
                var historial = await _context.Calculos
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                return View(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar el historial en PostgreSQL.");
                return View(new List<Calculo>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CargarCalculo(int id)
        {
            var calculo = await _context.Calculos.FindAsync(id);
            if (calculo == null)
            {
                return NotFound();
            }

            return Json(new { success = true, data = calculo });
        }

        [HttpPost]
        public async Task<IActionResult> GuardarHistorial([FromBody] CalculadoraTelaVM model)
        {
            if (model == null) return Json(new { success = false, message = "Datos inválidos" });

            try
            {
                // Recalculamos los valores completos antes de guardar
                var calculado = _calculadoraService.Calcular(model);

                // 1. CORRECCIÓN DE FECHA: Se guarda en formato UTC estándar
                var fechaActualUtc = DateTime.UtcNow;

                var entidad = new Calculo
                {
                    FechaCreacion = fechaActualUtc,
                    TipoProducto = calculado.TipoProducto,
                    UrdimbreTejido = calculado.UrdimbreTejido,
                    UrdimbreDenier = calculado.UrdimbreDenier,
                    TramaTejido = calculado.TramaTejido,
                    TramaDenier = calculado.TramaDenier,
                    Laminado = calculado.Laminado,
                    AnchoRefuerzoFactor = calculado.AnchoRefuerzoFactor,
                    Ancho = calculado.Ancho,
                    Corte = calculado.Corte,
                    MaquinaNumero = calculado.MaquinaNumero,
                    ResistenciaUrdimbre = calculado.ResistenciaUrdimbre,
                    PesoUrdimbre = calculado.PesoUrdimbre,
                    PorcentajeUrdimbre = calculado.PorcentajeUrdimbre,
                    ResistenciaTrama = calculado.ResistenciaTrama,
                    PesoTrama = calculado.PesoTrama,
                    PorcentajeTrama = calculado.PorcentajeTrama,
                    UrdimbreRefuerzoResistencia = calculado.UrdimbreRefuerzoResistencia,
                    PesoTejidoBase = calculado.PesoTejidoBase,
                    PesoConLaminado = calculado.PesoConLaminado,
                    PesoConRefuerzo = calculado.PesoConRefuerzo,
                    PesoMetroLineal = calculado.PesoMetroLineal,

                    // 2. CORRECCIÓN DE GMP: Asegura tomar el valor calculado o usa el peso con refuerzo para evitar el 0.0
                    PesoPorBolsa = calculado.PesoPorBolsa > 0 ? calculado.PesoPorBolsa : calculado.PesoConRefuerzo,
                    
                    ResumenFicha = calculado.ResumenFicha
                };

                _context.Calculos.Add(entidad);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = entidad.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar registro en la base de datos.");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
