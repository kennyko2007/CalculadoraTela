using CalculadoraTela.ViewModels;

namespace CalculadoraTela.Services;

public class CalculadoraService
{
    public CalculadoraTelaVM Calcular(CalculadoraTelaVM vm)
    {
        if (vm == null) return new CalculadoraTelaVM();

        // 1. Validación del Ancho de Refuerzo (Rango estricto de 0 a 12)
        if (vm.AnchoRefuerzoFactor < 0) vm.AnchoRefuerzoFactor = 0;
        if (vm.AnchoRefuerzoFactor > 12) vm.AnchoRefuerzoFactor = 12;

        // 2. Cálculo de Cantidad de Conos / Máquina (Fórmula celda B6 de Hoja 2)
        // Equivalente a: EVEN((Ancho * (FactorTipoProducto * 10)) / (CintaUrdimbre / 1.035))
        decimal factorTipoProducto = vm.TipoProducto?.ToLower() == "tubular" ? 2.0m : 1.0m;
        
        if (vm.CintaUrdimbre > 0)
        {
            decimal valorConos = (vm.Ancho * (factorTipoProducto * 10m)) / (vm.CintaUrdimbre / 1.035m);
            int parInferior = (int)Math.Ceiling(valorConos);
            if (parInferior % 2 != 0) parInferior++;
            vm.MaquinaNumero = parInferior; // Cantidad de conos resultante
        }
        else
        {
            vm.MaquinaNumero = 18; // Valor por defecto de seguridad si es 0
        }

        // 3. Resistencias (Celdas E3, E4, E5 de Hoja 2)
        // Fórmula: =+(Tejido * 2) * (Denier / 1000) * 4.7
        vm.ResistenciaUrdimbre = (vm.UrdimbreTejido * 2m) * (vm.UrdimbreDenier / 1000m) * 4.7m;
        vm.ResistenciaTrama = (vm.TramaTejido * 2m) * (vm.TramaDenier / 1000m) * 4.7m;
        vm.UrdimbreRefuerzoResistencia = (vm.UrdimbreRefuerzoTejido * 2m) * (vm.DenierRefuerzo / 1000m) * 4.7m;

        // 4. Pesos Base (Celdas G3 y G4 de Hoja 2)
        // Fórmula: =(Tejido * Denier) / 228.6
        vm.PesoUrdimbre = (vm.UrdimbreTejido * vm.UrdimbreDenier) / 228.6m;
        vm.PesoTrama = (vm.TramaTejido * vm.TramaDenier) / 228.6m;

        // 5. Porcentajes de Participación (Celdas H3 y H4 de Hoja 2)
        // Fórmula: =+((Peso * 100) / (PesoUrdimbre + PesoTrama))
        decimal sumaPesosBase = vm.PesoUrdimbre + vm.PesoTrama;
        if (sumaPesosBase > 0)
        {
            vm.PorcentajeUrdimbre = (vm.PesoUrdimbre * 100m) / sumaPesosBase;
            vm.PorcentajeTrama = (vm.PesoTrama * 100m) / sumaPesosBase;
        }
        else
        {
            vm.PorcentajeUrdimbre = 0;
            vm.PorcentajeTrama = 0;
        }

        // 6. GM2 - Peso Tejido Base (Celda I3 de Hoja 2)
        // Fórmula: =+SUM(G3:G4) * 1.05
        vm.PesoTejidoBase = sumaPesosBase * 1.05m;

        // 7. GM2 (PP + LAM) - Peso con Laminado (Celda I6 de Hoja 2)
        // Fórmula: =+I3 + G6
        vm.PesoConLaminado = vm.PesoTejidoBase + vm.Laminado;

        // 8. Tabla Interna de Refuerzo (Rango M:N de Hoja 2 para valores de 0 a 12)
        decimal factorTablaRefuerzo = vm.AnchoRefuerzoFactor switch
        {
            0 => 1.00m,
            1 => 1.01m,
            2 => 1.02m,
            3 => 1.03m,
            4 => 1.04m,
            5 => 1.05m,
            6 => 1.06m,
            7 => 1.07m,
            8 => 1.08m,
            9 => 1.09m,
            10 => 1.10m,
            11 => 1.11m,
            12 => 1.12m,
            _ => 1.00m
        };

        // 9. GMP - Peso con Refuerzo (Celda I5 de Hoja 2)
        // Fórmula: =+VLOOKUP(H5, M:N, 2, FALSE) * I6
        vm.PesoConRefuerzo = factorTablaRefuerzo * vm.PesoConLaminado;

        // 10. GML - Peso Metro Lineal (Celda I12 de Hoja 2)
        // Fórmula: =+VLOOKUP(B9, K:L, 2, FALSE) * I11 * (B7 / 100)
        decimal multiplicadorProductoGML = vm.TipoProducto?.ToLower() == "tubular" ? 1.01m : 1.00m;
        vm.PesoMetroLineal = multiplicadorProductoGML * vm.PesoConRefuerzo * (vm.Ancho / 100m);

        return vm;
    }
}
