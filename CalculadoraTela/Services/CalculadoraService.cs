using CalculadoraTela.ViewModels;

namespace CalculadoraTela.Services;

public class CalculadoraService
{
    // Tabla de factores de Refuerzo según el ancho de refuerzo (cm)
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
        { 11, 1.11 },
        { 12, 1.12 }
    };

    // Tabla de factores por Tipo de Producto (Plana = 1.0, Tubular = 2.0)
    private readonly Dictionary<string, double> _tablaTipoProducto = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Plana", 1.0 },
        { "Tubular", 2.0 }
    };

    public CalculadoraTelaVM CalcularValores(CalculadoraTelaVM model)
    {
        // 1. Cálculos de Urdimbre Base
        model.ResistenciaUrdimbre = (model.UrdimbreTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;
        model.PesoUrdimbre = (model.UrdimbreTejido * model.UrdimbreDenier) / 228.6;

        // 2. Cálculos de Trama Base
        model.ResistenciaTrama = (model.TramaTejido * 2.0) * (model.TramaDenier / 1000.0) * 4.7;
        model.PesoTrama = (model.TramaTejido * model.TramaDenier) / 228.6;

        // 3. Porcentajes de Peso
        double pesoTotalBaseSinPorcentaje = model.PesoUrdimbre + model.PesoTrama;
        if (pesoTotalBaseSinPorcentaje > 0)
        {
            model.PorcentajeUrdimbre = (model.PesoUrdimbre * 100.0) / pesoTotalBaseSinPorcentaje;
            model.PorcentajeTrama = (model.PesoTrama * 100.0) / pesoTotalBaseSinPorcentaje;
        }

        // 4. Peso Tejido Base con factor 1.05 (GM2)
        model.PesoTejidoBase = pesoTotalBaseSinPorcentaje * 1.05;

        // 5. Refuerzo / Urdimbre Extra (Fila 3 de la tabla: Tejido x2, misma cinta y denier)
        double urdimbreRefuerzoTejido = model.UrdimbreTejido * 2.0;
        model.UrdimbreRefuerzoResistencia = (urdimbreRefuerzoTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;

        // 6. Obtener Factor de Refuerzo según los centímetros de ancho de refuerzo seleccionados
        int codigoRefuerzoInt = (int)model.AnchoRefuerzoFactor; 
        double factorRef = _tablaFactoresRefuerzo.TryGetValue(codigoRefuerzoInt, out double fRef) ? fRef : 1.00;

        // 7. Peso con Laminado (GM2 PP+LAM) = Peso Tejido Base + Laminado
        model.PesoConLaminado = model.PesoTejidoBase + model.Laminado;

        // 8. Peso con Refuerzo (GMP) = Peso Tejido Base * Factor de Refuerzo
        model.PesoConRefuerzo = model.PesoTejidoBase * factorRef;

        // 9. Factor del Tipo de Producto (Plana / Tubular)
        double factorTipo = 2.0; // Por defecto Tubular
        if (!string.IsNullOrEmpty(model.TipoProducto) && _tablaTipoProducto.TryGetValue(model.TipoProducto, out double valTipo))
        {
            factorTipo = valTipo;
        }

        // 10. Peso Metro Lineal (GML) = FactorTipo * PesoConRefuerzo * (Ancho / 100.0)
        model.PesoMetroLineal = factorTipo * model.PesoConRefuerzo * (model.Ancho / 100.0);

        // 11. Peso por Bolsa (gr/Bol)
        double proporcionBolsa = (model.Corte > 0) ? (model.Corte / 100.0) : (model.Ancho / 100.0);
        model.PesoPorBolsa = model.PesoMetroLineal * proporcionBolsa;

        // 12. Generación del Resumen de Ficha Técnica (A11 del Excel)
        int g4Int = (int)Math.Round(model.PesoConLaminado);
        int g5Int = (int)Math.Round(model.PesoConRefuerzo);
        int g7Int = (int)Math.Round(model.PesoMetroLineal);
        double g9Redondeado = Math.Round(model.PesoPorBolsa, 2);

        model.ResumenFicha = $"{model.Ancho}x{model.Corte} \\ {model.UrdimbreTejido}x{model.TramaTejido} \\ {g4Int}gm2 \\ {g5Int}gmp \\ {g7Int}gml \\ {g9Redondeado}gr/Bol.";

        return model;
    }
}
