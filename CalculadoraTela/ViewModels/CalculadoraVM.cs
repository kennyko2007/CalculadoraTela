using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.ViewModels;

public class CalculadoraTelaVM
{
    [Display(Name = "Tipo de Producto")]
    public string TipoProducto { get; set; } = "Tubular"; // Tubular o Plana

    // --- URDIMBRE BASE ---
    [Display(Name = "Urdimbre Tejido")]
    public double UrdimbreTejido { get; set; } = 10.0;

    [Display(Name = "Cinta Urdimbre")]
    public double CintaUrdimbre { get; set; } = 2.5;

    [Display(Name = "Urdimbre Denier")]
    public int UrdimbreDenier { get; set; } = 680;

    // --- TRAMA BASE ---
    [Display(Name = "Trama Tejido")]
    public double TramaTejido { get; set; } = 6.0;

    [Display(Name = "Cinta Trama")]
    public double CintaTrama { get; set; } = 4.0;

    [Display(Name = "Trama Denier")]
    public int TramaDenier { get; set; } = 900;

    // --- URDIMBRE REFUERZO (Fila extra) ---
    [Display(Name = "Urdimbre Refuerzo Tejido")]
    public double UrdimbreRefuerzoTejido { get; set; } = 20.0;

    [Display(Name = "Cinta Refuerzo")]
    public double CintaRefuerzo { get; set; } = 2.0;

    [Display(Name = "Denier Refuerzo")]
    public int DenierRefuerzo { get; set; } = 680;

    [Display(Name = "Ancho Refuerzo (cm)")]
    public double AnchoRefuerzoFactor { get; set; } = 5.0; // cm de refuerzo

    // --- CONFIGURACIÓN Y ANCHO ---
    [Display(Name = "Ancho (cm)")]
    public double Ancho { get; set; } = 56.0;

    [Display(Name = "Laminado")]
    public double Laminado { get; set; } = 10.0;

    [Display(Name = "Corte / Largo (cm)")]
    public double Corte { get; set; } = 100.0;

    [Display(Name = "Número de Máquina")]
    public int MaquinaNumero { get; set; } = 18;

    // --- SALIDAS / RESULTADOS ---
    public double ResistenciaUrdimbre { get; set; }
    public double PesoUrdimbre { get; set; }
    public double PorcentajeUrdimbre { get; set; }

    public double ResistenciaTrama { get; set; }
    public double PesoTrama { get; set; }
    public double PorcentajeTrama { get; set; }

    public double UrdimbreRefuerzoResistencia { get; set; }

    public double PesoTejidoBase { get; set; }    // GM2
    public double PesoConLaminado { get; set; }   // GM2 (PP+LAM)
    public double PesoConRefuerzo { get; set; }   // GMP
    public double PesoMetroLineal { get; set; }   // GML
    public double PesoPorBolsa { get; set; }      // gr/Bol

    public string ResumenFicha { get; set; } = string.Empty;
}
