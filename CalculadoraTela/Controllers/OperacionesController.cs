using Microsoft.AspNetCore.Mvc;

namespace CalculadoraTela.Controllers
{
    public class OperacionesController : Controller
    {
        // Pestaña independiente de Rendimiento
        public IActionResult Rendimiento() => View();

        // Pestaña independiente para Asistencia / Ausencias
        public IActionResult Ausencias(string tipo = "Falta")
        {
            ViewBag.TipoAusencia = tipo;
            return View();
        }

        // Endpoint liviano en memoria para los ejercicios de cálculo del Excel
        [HttpPost]
        public IActionResult ProcesarOperacion([FromBody] OperacionRequest model)
        {
            if (model == null || model.Valores == null || !model.Valores.Any())
                return Json(new { success = true, resultado = 0, totalFilas = 0 });

            double resultado = model.TipoOperacion.ToLower() switch
            {
                "rendimiento" => model.Valores.Average(),
                "suma" => model.Valores.Sum(),
                _ => model.Valores.Count(v => v > 0)
            };

            return Json(new { 
                success = true, 
                resultado = Math.Round(resultado, 2), 
                totalFilas = model.Valores.Count 
            });
        }
    }

    public class OperacionRequest
    {
        public string TipoOperacion { get; set; } = string.Empty;
        public List<double> Valores { get; set; } = new();
    }
}
