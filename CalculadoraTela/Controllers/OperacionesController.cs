using Microsoft.AspNetCore.Mvc;

namespace CalculadoraTela.Controllers;

public class OperacionesController : Controller
{
    public IActionResult Rendimiento()
    {
        return View();
    }

    public IActionResult Ausencias(string tipo = "Asistencia")
    {
        ViewBag.TipoFiltro = tipo;
        return View();
    }
}
