using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.ViewModels;

public class CalculadoraTelaVM
{
    // --- Entradas Principales ---
    [Display(Name = "Urdimbre Tejido")]
    public double UrdimbreTejido { get; set; } = 13.0;

    [Display(Name = "Cinta Urdimbre")]
    public double CintaUrdimbre { get; set; } = 2.5;

    [Display(Name = "Urdimbre Denier")]
    public double UrdimbreDenier { get; set; } = 1750.0;

    [Display(Name = "Trama Tejido")]
    public double TramaTejido { get; set; } = 11.5;

    [Display(Name = "Cinta Trama")]
    public double CintaTrama { get; set; } = 2.5;

    [Display(Name = "Trama Denier")]
    public double TramaDenier { get; set; } = 1500.0;

    // --- Entradas de Configuración ---
    [Display(Name = "Tipo de Producto")]
    public string TipoProducto { get; set; } = "Tubular"; // Plana o Tubular

    [Display(Name = "Laminado")]
    public double Laminado { get; set; } = 0.0;

    [Display(Name = "Factor Ancho Refuerzo")]
    public double AnchoRefuerzoFactor { get; set; } = 0; // Del 0 al 12

    [Display(Name = "Ancho (cm)")]
    public double Ancho { get; set; } = 184.0;

    [Display(Name = "Lado")]
    public double Lado { get; set; } = 2.0;

    [Display(Name = "Corte")]
    public double Corte { get; set; } = 0.0;

    [Display(Name = "Costura")]
    public double Costura { get; set; } = 2.0;

    // --- Entradas Producción ---
    [Display(Name = "Número de Máquina")]
    public int MaquinaNumero { get; set; } = 18;

    // --- Salidas / Resultados ---
    public double ResistenciaUrdimbre { get; set; }
    public double PesoUrdimbre { get; set; }
    public double PorcentajeUrdimbre { get; set; }

    public double ResistenciaTrama { get; set; }
    public double PesoTrama { get; set; }
    public double PorcentajeTrama { get; set; }

    // Refuerzos (Fila 5 y 6 del Excel)
    public double UrdimbreRefuerzoTejido => UrdimbreTejido * 2;
    public double UrdimbreRefuerzoResistencia { get; set; }
    public double TramaRefuerzoResistencia { get; set; }

    // Pesos Totales
    public double PesoTejidoBase { get; set; }  // G2
    public double PesoConLaminado { get; set; } // G4
    public double PesoConRefuerzo { get; set; } // G5
    public double PesoMetroLineal { get; set; } // G7
    public double PesoPorBolsa { get; set; }    // G9

    // Producción
    public double FactorMaquina { get; set; }
    public double ProduccionEstimada { get; set; }

    // Nomenclatura del Excel (Celda A11)
    public string ResumenFicha { get; set; } = string.Empty;
}
