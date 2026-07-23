using CalculadoraTela.ViewModels;

namespace CalculadoraTela.Services;

public class CalculadoraService
{
    // Diccionario de equivalencia Máquina -> Constante (Tabla Hoja1 del Excel)
    private readonly Dictionary<int, double> _tablaMaquinas = new()
    {
        { 1, 448 }, { 2, 448 }, { 3, 448 }, { 4, 448 }, { 5, 448 },
        { 6, 448 }, { 7, 448 }, { 8, 448 }, { 9, 540 }, { 10, 540 },
        { 11, 560 }, { 12, 560 }, { 13, 640 }, { 14, 640 }, { 15, 636 },
        { 16, 636 }, { 17, 780 }, { 18, 780 }, { 19, 780 }, { 20, 780 },
        { 21, 480 }, { 22, 480 }, { 23, 480 }, { 24, 480 }
    };

    // Tabla de factores de Refuerzo (Columnas M y N del Excel)
    private readonly Dictionary<int, double> _tablaFactoresRefuerzo = new()
    {
        { 0, 1.00 },
        { 1, 1.01 },
        { 2, 1.02 },
        { 3, 1.03 },
        { 4, 1.04 },
        { 5, 1.05 },
        { 6, 1.06 },
        { 7, 1.07 },
        { 8, 1.08 },
        { 9, 1.09 },
        { 10, 1.10 },
        { 11, 1.11 }
    };

    // Tabla de factores por Tipo de Producto (Plana / Tubular -> Columnas K y L del Excel)
    private readonly Dictionary<string, double> _tablaTipoProducto = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Plana", 1.0 },
        { "Tubular", 2.0 }
    };

    public CalculadoraTelaVM CalcularValores(CalculadoraTelaVM model)
    {
        // 1. Cálculos de Urdimbre Base (E3 = (B3*2)*(D3/1000)*4.7, G3 = (B3*D3)/228.6)
        model.ResistenciaUrdimbre = (model.UrdimbreTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;
        model.PesoUrdimbre = (model.UrdimbreTejido * model.UrdimbreDenier) / 228.6;

        // 2. Cálculos de Trama Base (E4 = (B4*2)*(D4/1000)*4.7, G4 = (B4*D4)/228.6)
        model.ResistenciaTrama = (model.TramaTejido * 2.0) * (model.TramaDenier / 1000.0) * 4.7;
        model.PesoTrama = (model.TramaTejido * model.TramaDenier) / 228.6;

        // 3. Porcentajes (H3, H4: (Peso / (PesoUrd + PesoTram)) * 100)
        double pesoTotalBaseSinPorcentaje = model.PesoUrdimbre + model.PesoTrama;
        if (pesoTotalBaseSinPorcentaje > 0)
        {
            model.PorcentajeUrdimbre = (model.PesoUrdimbre * 100.0) / pesoTotalBaseSinPorcentaje;
            model.PorcentajeTrama = (model.PesoTrama * 100.0) / pesoTotalBaseSinPorcentaje;
        }

        // 4. Peso Tejido Base con 5% Adicional (I3: SUM(G3:G4) * 1.05)
        model.PesoTejidoBase = pesoTotalBaseSinPorcentaje * 1.05;

        // 5. Refuerzo / Urdimbre Extra (Fila 5 del Excel)
        double urdimbreRefuerzoTejido = model.UrdimbreTejido * 2.0;
        model.UrdimbreRefuerzoResistencia = (urdimbreRefuerzoTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;

        // 6. Obtener Factor de Refuerzo según selección (Simula VLOOKUP de celdas M:N)
        int codigoRefuerzoInt = (int)model.AnchoRefuerzoFactor; // O el campo que represente el índice de refuerzo (0, 1, 2...)
        if (_tablaFactoresRefuerzo.TryGetValue(codigoRefuerzoInt, out double factorRef))
        {
            model.AnchoRefuerzoFactor = factorRef;
        }
        else
        {
            model.AnchoRefuerzoFactor = 1.00;
        }

        // 7. Peso con Laminado (I6: I3 + G6)
        model.PesoConLaminado = model.PesoTejidoBase + model.Laminado;

        // 8. Peso con Refuerzo (I5: VLOOKUP del factor * Peso con Laminado o Tejido Base según fórmula)
        model.PesoConRefuerzo = model.PesoTejidoBase * model.AnchoRefuerzoFactor; // Ajustado al modelo de celdas del Excel

        // 9. Factor del Tipo de Producto (Plana / Tubular -> VLOOKUP en K:L)
        double factorTipo = 2.0; // Por defecto Tubular
        if (!string.IsNullOrEmpty(model.TipoProducto) && _tablaTipoProducto.TryGetValue(model.TipoProducto, out double valTipo))
        {
            factorTipo = valTipo;
        }

        // 10. Peso Metro Lineal (Gml - Celda I12 del Excel: VLOOKUP(Tipo) * PesoRefuerzo * (Ancho/100))
        model.PesoMetroLineal = factorTipo * model.PesoConRefuerzo * (model.Ancho / 100.0);

        // 11. Peso por Bolsa (gr/Bol - Celda I7 / I12 adaptada)
        // Fórmula del Excel: FactorTipo * PesoRefuerzo * (Ancho/100) * (BolsaProporcion)
        double proporcionBolsa = (model.Corte > 0) ? (model.Corte / 100.0) : (model.Ancho / 100.0);
        model.PesoPorBolsa = model.PesoMetroLineal * proporcionBolsa;

        // 12. Producción de Máquina (Basado en Hoja1 del Excel)
        if (_tablaMaquinas.TryGetValue(model.MaquinaNumero, out double factorMaq))
        {
            model.FactorMaquina = factorMaq;
        }
        else
        {
            model.FactorMaquina = 540; // Valor por defecto
        }

        if (model.Engranaje > 0)
        {
            model.ProduccionEstimada = (model.FactorMaquina / model.Engranaje) * model.Horas;
        }

        // 13. Generación del Resumen de Ficha Técnica (A11 del Excel)
        int g4Int = (int)Math.Round(model.PesoConLaminado);
        int g5Int = (int)Math.Round(model.PesoConRefuerzo);
        int g7Int = (int)Math.Round(model.PesoMetroLineal);
        double g9Redondeado = Math.Round(model.PesoPorBolsa, 2);

        model.ResumenFicha = $"{model.Ancho}x{model.Corte} \\ {model.UrdimbreTejido}x{model.TramaTejido} \\ {g4Int}gm2 \\ {g5Int}gmp \\ {g7Int}gml \\ {g9Redondeado}gr/Bol.";

        return model;
    }
}
