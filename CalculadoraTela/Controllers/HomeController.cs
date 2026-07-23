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

[HttpPost]
public async Task<IActionResult> GuardarHistorial([FromBody] CalculadoraTelaVM model)
{
    if (model == null) return Json(new { success = false, message = "Datos inválidos" });

    try
    {
        // Recalculamos los valores completos antes de guardar
        var calculado = _calculadoraService.Calcular(model);

        // 1. CORRECCIÓN DE FECHA: Se guarda en formato UTC real restando/sumando la zona horaria del servidor
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

            // 2. CORRECCIÓN DE GMP: Aseguramos tomar el valor calculado de PesoConRefuerzo si PesoPorBolsa viene en 0
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
