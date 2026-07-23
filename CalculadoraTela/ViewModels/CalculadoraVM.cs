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
    public int UrdimbreDenier { get; set; } = 1750;

    [Display(Name = "Trama Tejido")]
    public double TramaTejido { get; set; } = 11.5;

    [Display(Name = "Cinta Trama")]
    public double CintaTrama { get; set; } = 2.5;

    [Display(Name = "Trama Denier")]
    public int TramaDenier { get; set; } = 1500;

    // --- Entradas de Configuración ---
    [Display(Name = "Tipo de Producto")]
    public string TipoProducto { get; set; } = "Tubular"; // Tubular o Plana

    [Display(Name = "Laminado")]
    public double Laminado { get; set; } = 0.0;

    [Display(Name = "Factor Ancho Refuerzo")]
    public double AnchoRefuerzoFactor { get; set; } = 0; // Del 0 al 12

    [Display(Name = "Ancho (cm)")]
    public double Ancho { get; set; } = 184.0;

    [Display(Name = "Lado")]
    public int Lado { get; set; } = 2;

    [Display(Name = "Corte")]
    public double Corte { get; set; } = 0.0;

    [Display(Name = "Costura")]
    public double Costura { get; set; } = 2.0;

    // --- Entradas Producción ---
    [Display(Name = "Número de Máquina")]
    public int MaquinaNumero { get; set; } = 18;

    // --- Salidas / Resultados (Urdimbre) ---
    public double ResistenciaUrdimbre { get; set; }
    public double PesoUrdimbre { get; set; }
    public double PorcentajeUrdimbre { get; set; }

    // --- Salidas / Resultados (Trama) ---
    public double ResistenciaTrama { get; set; }
    public double PesoTrama { get; set; }
    public double PorcentajeTrama { get; set; }

    // --- Refuerzos / Adicionales ---
    public double UrdimbreRefuerzoResistencia { get; set; }
    public double TramaRefuerzoResistencia { get; set; }

    // --- Pesos Totales y Parciales ---
    public double PesoTejidoBase { get; set; }    // GM2
    public double PesoConLaminado { get; set; }   // GM2 (PP+LAM)
    public double PesoConRefuerzo { get; set; }   // GMP
    public double PesoMetroLineal { get; set; }   // GML
    public double PesoPorBolsa { get; set; }      // gr/Bol

    // --- Producción y Conos ---
    public int CantidadConos { get; set; }
    public double FactorMaquina { get; set; }
    public double ProduccionEstimada { get; set; }

    // --- Resumen de Ficha Técnica ---
    public string ResumenFicha { get; set; } = string.Empty;
}
