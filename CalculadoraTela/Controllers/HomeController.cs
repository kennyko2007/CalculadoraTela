using CalculadoraTela.Data;
using CalculadoraTela.Models;
using CalculadoraTela.Services;
using CalculadoraTela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CalculadoraTela.Controllers
{
    [Authorize]
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

        // Id del usuario con sesión iniciada (viene de la cookie de Identity).
        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

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
                var query = _context.Calculos.AsQueryable();

                // Cada usuario ve solo sus propios registros; el administrador ve todos.
                if (!User.IsInRole("Admin"))
                {
                    query = query.Where(c => c.UserId == UserId);
                }

                var historial = await query
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

            if (!User.IsInRole("Admin") && calculo.UserId != UserId)
            {
                return Forbid();
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
                    UserId = UserId,
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

        [HttpPost]
        public async Task<IActionResult> EliminarRegistro(int id)
        {
            try
            {
                var calculo = await _context.Calculos.FindAsync(id);
                if (calculo == null)
                {
                    return Json(new { success = false, message = "El registro no existe." });
                }

                if (!User.IsInRole("Admin") && calculo.UserId != UserId)
                {
                    return Json(new { success = false, message = "No tienes permiso para eliminar este registro." });
                }

                _context.Calculos.Remove(calculo);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el registro {Id} del historial.", id);
                return Json(new { success = false, message = "Ocurrió un error al eliminar el registro." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VaciarHistorial()
        {
            try
            {
                // Cada usuario solo vacía sus propios registros (incluso si es admin,
                // para no borrar por accidente el historial de otras personas).
                await _context.Calculos
                    .Where(c => c.UserId == UserId)
                    .ExecuteDeleteAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vaciar el historial completo.");
                return Json(new { success = false, message = "Ocurrió un error al vaciar el historial." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
