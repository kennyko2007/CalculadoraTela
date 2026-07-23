using CalculadoraTela.ViewModels;

namespace CalculadoraTela.Services
{
    public class CalculadoraService
    {
        public CalculadoraTelaVM CalcularValores(CalculadoraTelaVM model)
        {
            if (model == null) return new CalculadoraTelaVM();

            // 1. Factor según el Tipo de Producto (Plana = 1, Tubular = 2)
            decimal factorProducto = model.TipoProducto == "Tubular" ? 2.0m : 1.0m;

            // 2. Cálculo de Resistencia y Peso de Urdimbre
            model.ResistenciaUrdimbre = (model.UrdimbreDenier > 0 && model.CintaUrdimbre > 0) 
                ? (model.UrdimbreTejido * model.UrdimbreDenier * model.CintaUrdimbre) / 1000.0m : 0; // Ajustar según fórmula base exacta
            
            // Peso Urdimbre Base (gm2)
            model.PesoUrdimbre = (model.UrdimbreTejido > 0 && model.UrdimbreDenier > 0) 
                ? (model.UrdimbreTejido * model.UrdimbreDenier) / 9000.0m * factorProducto : 0; // Fórmula estándar de peso textil

            // 3. Cálculo de Resistencia y Peso de Trama
            model.ResistenciaTrama = (model.TramaDenier > 0 && model.CintaTrama > 0) 
                ? (model.TramaTejido * model.TramaDenier * model.CintaTrama) / 1000.0m : 0;

            // Peso Trama Base (gm2)
            model.PesoTrama = (model.TramaTejido > 0 && model.TramaDenier > 0) 
                ? (model.TramaTejido * model.TramaDenier) / 9000.0m * factorProducto : 0;

            // 4. Porcentajes de participación
            decimal totalPesoTejido = model.PesoUrdimbre + model.PesoTrama;
            if (totalPesoTejido > 0)
            {
                model.PorcentajeUrdimbre = (model.PesoUrdimbre / totalPesoTejido) * 100.0m;
                model.PorcentajeTrama = (model.PesoTrama / totalPesoTejido) * 100.0m;
            }
            else
            {
                model.PorcentajeUrdimbre = 0;
                model.PorcentajeTrama = 0;
            }

            // 5. Urdimbre Refuerzo / Resistencia de Refuerzo
            model.UrdimbreRefuerzoResistencia = (model.DenierRefuerzo > 0 && model.CintaRefuerzo > 0) 
                ? (model.UrdimbreRefuerzoTejido * model.DenierRefuerzo * model.CintaRefuerzo) / 1000.0m : 0;

            // 6. Peso Tejido Base (GM2)
            model.PesoTejidoBase = totalPesoTejido;

            // 7. Peso con Laminado (GM2 PP+LAM)
            model.PesoConLaminado = model.PesoTejidoBase + model.Laminado;

            // 8. NUEVO: GMP (Peso con Refuerzo) -> FactorRefuerzo * PesoConLaminado
            decimal factorRefuerzo = 1.0m + (model.AnchoRefuerzoFactor * 0.01m);
            model.PesoConRefuerzo = factorRefuerzo * model.PesoConLaminado;

            // 9. NUEVO: GML (Gramos por Metro Lineal) -> FactorProducto * GMP * (Ancho / 100)
            model.PesoMetroLineal = factorProducto * model.PesoConRefuerzo * (model.Ancho / 100.0m);

            // 10. NUEVO: Cantidad de Conos (REDONDEA.PAR)
            if (model.UrdimbreTejido > 0)
            {
                decimal calculoConos = (model.Ancho * (factorProducto * 10.0m)) / (model.UrdimbreTejido / 1.035m);
                int conosRedondeados = (int)Math.Ceiling(calculoConos);
                if (conosRedondeados % 2 != 0) conosRedondeados += 1;
                model.MaquinaNumero = conosRedondeados;
            }

            // 11. NUEVO: Concatenaciones automáticas para el Resumen (Tejido y Denier)
            // Asegúrate de tener estas propiedades en tu ViewModel o concatenarlas directo en el JSON
            // model.ResumenTejido = $"{model.UrdimbreTejido}x{model.TramaTejido}";
            // model.ResumenDenier = $"{model.UrdimbreDenier}x{model.TramaDenier}";

            return model;
        }
    }
}
