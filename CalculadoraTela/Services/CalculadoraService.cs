using CalculadoraTela.ViewModels;

namespace CalculadoraTela.Services;

public class CalculadoraService
{
    private readonly Dictionary<int, double> _tablaFactoresRefuerzo = new()
    {
        { 0, 1.00 }, { 1, 1.01 }, { 2, 1.02 }, { 3, 1.03 },
        { 4, 1.04 }, { 5, 1.05 }, { 6, 1.06 }, { 7, 1.07 },
        { 8, 1.08 }, { 9, 1.09 }, { 10, 1.10 }, { 11, 1.11 }, { 12, 1.12 }
    };

    private readonly Dictionary<string, double> _tablaTipoProducto = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Plana", 1.0 },
        { "Tubular", 2.0 }
    };

    public CalculadoraTelaVM CalcularValores(CalculadoraTelaVM model)
    {
        // 1. Urdimbre Base
        model.ResistenciaUrdimbre = (model.UrdimbreTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;
        model.PesoUrdimbre = (model.UrdimbreTejido * model.UrdimbreDenier) / 228.6;

        // 2. Trama Base
        model.ResistenciaTrama = (model.TramaTejido * 2.0) * (model.TramaDenier / 1000.0) * 4.7;
        model.PesoTrama = (model.TramaTejido * model.TramaDenier) / 228.6;

        // 3. Porcentajes
        double pesoTotalBaseSinPorcentaje = model.PesoUrdimbre + model.PesoTrama;
        if (pesoTotalBaseSinPorcentaje > 0)
        {
            model.PorcentajeUrdimbre = (model.PesoUrdimbre * 100.0) / pesoTotalBaseSinPorcentaje;
            model.PorcentajeTrama = (model.PesoTrama * 100.0) / pesoTotalBaseSinPorcentaje;
        }

        // 4. Peso Tejido Base (GM2) con factor 1.05
        model.PesoTejidoBase = pesoTotalBaseSinPorcentaje * 1.05;

        // 5. Urdimbre Refuerzo Resistencia
        model.UrdimbreRefuerzoResistencia = (model.UrdimbreRefuerzoTejido * 2.0) * (model.DenierRefuerzo / 1000.0) * 4.7;

        // 6. Factor de Refuerzo por cm
        int cmRefuerzo = (int)Math.Round(model.AnchoRefuerzoFactor);
        double factorRef = _tablaFactoresRefuerzo.TryGetValue(cmRefuerzo, out double fRef) ? fRef : 1.05;

        // 7. Pesos Adicionales
        model.PesoConLaminado = model.PesoTejidoBase + model.Laminado;
        model.PesoConRefuerzo = model.PesoTejidoBase * factorRef;

        // 8. Tipo de Producto y Peso Metro Lineal (GML)
        double factorTipo = _tablaTipoProducto.TryGetValue(model.TipoProducto, out double valTipo) ? valTipo : 2.0;
        model.PesoMetroLineal = factorTipo * model.PesoConRefuerzo * (model.Ancho / 100.0);

        // 9. Peso por Bolsa
        double proporcionBolsa = (model.Corte > 0) ? (model.Corte / 100.0) : (model.Ancho / 100.0);
        model.PesoPorBolsa = model.PesoMetroLineal * proporcionBolsa;

        // 10. Resumen Ficha
        int g4Int = (int)Math.Round(model.PesoConLaminado);
        int g5Int = (int)Math.Round(model.PesoConRefuerzo);
        int g7Int = (int)Math.Round(model.PesoMetroLineal);
        double g9Redondeado = Math.Round(model.PesoPorBolsa, 2);

        model.ResumenFicha = $"{model.Ancho}x{model.Corte} \\ {model.UrdimbreTejido}x{model.TramaTejido} \\ {g4Int}gm2 \\ {g5Int}gmp \\ {g7Int}gml \\ {g9Redondeado}gr/Bol.";

        return model;
    }
}
