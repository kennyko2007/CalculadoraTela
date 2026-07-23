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
        { 16, 636 }, { 17, 780 }, { 18, 780 }, { 19, 780 }, { 20, 780 }
    };

    public CalculadoraTelaVM CalcularValores(CalculadoraTelaVM model)
    {
        // 1. Cálculos de Urdimbre Base (D2, E2)
        model.ResistenciaUrdimbre = (model.UrdimbreTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;
        model.PesoUrdimbre = (model.UrdimbreTejido * model.UrdimbreDenier) / 228.6;

        // 2. Cálculos de Trama Base (D3, E3)
        model.ResistenciaTrama = (model.TramaTejido * 2.0) * (model.TramaDenier / 1000.0) * 4.7;
        model.PesoTrama = (model.TramaTejido * model.TramaDenier) / 228.6;

        // 3. Porcentajes (F2, F3)
        double pesoTotalBaseSinPorcentaje = model.PesoUrdimbre + model.PesoTrama;
        if (pesoTotalBaseSinPorcentaje > 0)
        {
            model.PorcentajeUrdimbre = (model.PesoUrdimbre * 100.0) / pesoTotalBaseSinPorcentaje;
            model.PorcentajeTrama = (model.PesoTrama * 100.0) / pesoTotalBaseSinPorcentaje;
        }

        // 4. Peso Tejido Base con 5% Adicional (G2: SUM(E2:E3) * 1.05)
        model.PesoTejidoBase = pesoTotalBaseSinPorcentaje * 1.05;

        // 5. Peso con Laminado (G4: G2 + E4)
        model.PesoConLaminado = model.PesoTejidoBase + model.Laminado;

        // 6. Refuerzo Resistencia (D5, D6)
        double urdimbreRefuerzoTejido = model.UrdimbreTejido * 2.0;
        model.UrdimbreRefuerzoResistencia = (urdimbreRefuerzoTejido * 2.0) * (model.UrdimbreDenier / 1000.0) * 4.7;
        model.TramaRefuerzoResistencia = (model.TramaTejido * 2.0) * (model.TramaDenier / 1000.0) * 4.7;

        // 7. Peso con Refuerzo (G5: G4 * F5)
        model.PesoConRefuerzo = model.PesoConLaminado * model.AnchoRefuerzoFactor;

        // 8. Peso Metro Lineal (G7: G5 * ((Ancho / 100) * Lado))
        model.PesoMetroLineal = model.PesoConRefuerzo * ((model.Ancho / 100.0) * model.Lado);

        // 9. Peso por Bolsa (G9: G7 * ((Corte + Costura) / 100))
        model.PesoPorBolsa = model.PesoMetroLineal * ((model.Corte + model.Costura) / 100.0);

        // 10. Producción (D13: VLOOKUP(B13, Hoja1) / B14 * B15)
        if (_tablaMaquinas.TryGetValue(model.MaquinaNumero, out double factor))
        {
            model.FactorMaquina = factor;
        }
        else
        {
            model.FactorMaquina = 0;
        }

        if (model.Engranaje > 0)
        {
            model.ProduccionEstimada = (model.FactorMaquina / model.Engranaje) * model.Horas;
        }

        // 11. Generación del Resumen A11
        // Formato Excel: CONCATENATE(Ancho, "x", Corte-Costura, " \ ", UrdimbreTejido, "x", TramaTejido, " \ ", INT(G4), "gm2 ...")
        int g4Int = (int)Math.Floor(model.PesoConLaminado);
        int g5Int = (int)Math.Floor(model.PesoConRefuerzo);
        int g7Int = (int)Math.Floor(model.PesoMetroLineal);
        int g9Int = (int)Math.Floor(model.PesoPorBolsa);
        double restaCorteCostura = model.Corte - model.Costura;

        model.ResumenFicha = $"{model.Ancho}x{restaCorteCostura} \\ {model.UrdimbreTejido}x{model.TramaTejido} \\ {g4Int}gm2 \\ {g5Int}gmp \\ {g7Int}gml \\ {g9Int}gr/Bol.";

        return model;
    }
}
