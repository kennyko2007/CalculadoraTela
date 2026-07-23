using CalculadoraTela.ViewModels;

namespace CalculadoraTela.Services
{
    public class CalculadoraService
    {
        public CalculadoraTelaVM CalcularValores(CalculadoraTelaVM model)
        {
            if (model == null) return new CalculadoraTelaVM();

            // 1. Factor según el Tipo de Producto (Plana = 1.0, Tubular = 2.0)
            decimal factorProducto = model.TipoProducto == "Tubular" ? 2.0m : 1.0m;

            // 2. Cálculo de Resistencia y Peso de Urdimbre
            model.ResistenciaUrdimbre = (model.UrdimbreTejido > 0m && model.UrdimbreDenier > 0m) 
                ? (model.UrdimbreTejido * model.UrdimbreDenier * 0.0471m) : 0m;
            
            model.PesoUrdimbre = (model.UrdimbreTejido > 0m && model.UrdimbreDenier > 0m) 
                ? (model.UrdimbreTejido * model.UrdimbreDenier) / 450.0m : 0m;

            // 3. Cálculo de Resistencia y Peso de Trama
            model.ResistenciaTrama = (model.TramaTejido > 0m && model.TramaDenier > 0m) 
                ? (model.TramaTejido * model.TramaDenier * 0.0471m) : 0m;

            model.PesoTrama = (model.TramaTejido > 0m && model.TramaDenier > 0m) 
                ? (model.TramaTejido * model.TramaDenier) / 450.0m : 0m;

            // 4. Porcentajes de participación
            decimal totalPesoTejido = model.PesoUrdimbre + model.PesoTrama;
            if (totalPesoTejido > 0m)
            {
                model.PorcentajeUrdimbre = (model.PesoUrdimbre / totalPesoTejido) * 100.0m;
                model.PorcentajeTrama = (model.PesoTrama / totalPesoTejido) * 100.0m;
            }
            else
            {
                model.PorcentajeUrdimbre = 0m;
                model.PorcentajeTrama = 0m;
            }

            // 5. Urdimbre Refuerzo / Resistencia de Refuerzo
            model.UrdimbreRefuerzoResistencia = model.ResistenciaUrdimbre;

            // 6. Peso Tejido Base (GM2)
            model.PesoTejidoBase = totalPesoTejido;

            // 7. Peso con Laminado (GM2 PP+LAM)
            model.PesoConLaminado = model.PesoTejidoBase + model.Laminado;

            // 8. GMP (Peso con Refuerzo sumando directamente el factor de ancho de refuerzo del 0 al 12)
            model.PesoConRefuerzo = model.PesoConLaminado + model.AnchoRefuerzoFactor;

            // 9. GML (Gramos por Metro Lineal)
            model.PesoMetroLineal = (model.PesoConRefuerzo * model.Ancho * 2.0m) / 1000.0m;

            // 10. Cantidad de Conos (Equivalente exacto a REDONDEA.PAR de Excel)
            if (model.UrdimbreTejido > 0m && model.Ancho > 0m)
            {
                decimal calculoConos = (model.Ancho * (factorProducto * 10.0m)) / (model.UrdimbreTejido / 1.035m);
                int conosRedondeados = (int)Math.Ceiling(calculoConos / 2.0m) * 2;
                model.MaquinaNumero = conosRedondeados;
            }
            else
            {
                model.MaquinaNumero = 0;
            }

            return model;
        }
    }
}
