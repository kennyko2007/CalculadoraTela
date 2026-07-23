using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.ViewModels;

public class CalculadoraTelaVM
{
    [Display(Name = "Tipo de Producto")]
    public string TipoProducto { get; set; } = "Tubular"; // Tubular o Plana

    // --- URDIMBRE BASE ---
    [Display(Name = "Urdimbre Tejido")]
    public decimal UrdimbreTejido { get; set; } = 10.0m;

    [Display(Name = "Cinta Urdimbre")]
    public decimal CintaUrdimbre { get; set; } = 2.5m;

    [Display(Name = "Urdimbre Denier")]
    public decimal UrdimbreDenier { get; set; } = 680m;

    // --- TRAMA BASE ---
    [Display(Name = "Trama Tejido")]
    public decimal TramaTejido { get; set; } = 6.0m;

    [Display(Name = "Cinta Trama")]
    public decimal CintaTrama { get; set; } = 4.0m;

    [Display(Name = "Trama Denier")]
    public decimal TramaDenier { get; set; } = 900m;

    // --- URDIMBRE REFUERZO (Fila extra) ---
    [Display(Name = "Urdimbre Refuerzo Tejido")]
    public decimal UrdimbreRefuerzoTejido { get; set; } = 20.0m;

    [Display(Name = "Cinta Refuerzo")]
    public decimal CintaRefuerzo { get; set; } = 2.0m;

    [Display(Name = "Denier Refuerzo")]
    public decimal DenierRefuerzo { get; set; } = 680m;

    [Display(Name = "Ancho Refuerzo (cm)")]
    public decimal AnchoRefuerzoFactor { get; set; } = 5.0m; // cm de refuerzo

    // --- CONFIGURACIÓN Y ANCHO ---
    [Display(Name = "Ancho (cm)")]
    public decimal Ancho { get; set; } = 56.0m;

    [Display(Name = "Laminado")]
    public decimal Laminado { get; set; } = 10.0m;

    [Display(Name = "Corte / Largo (cm)")]
    public decimal Corte { get; set; } = 100.0m;

    [Display(Name = "Número de Máquina")]
    public int MaquinaNumero { get; set; } = 18;

    // --- SALIDAS / RESULTADOS ---
    public decimal ResistenciaUrdimbre { get; set; }
    public decimal PesoUrdimbre { get; set; }
    public decimal PorcentajeUrdimbre { get; set; }

    public decimal ResistenciaTrama { get; set; }
    public decimal PesoTrama { get; set; }
    public decimal PorcentajeTrama { get; set; }

    public decimal UrdimbreRefuerzoResistencia { get; set; }

    public decimal PesoTejidoBase { get; set; }    // GM2
    public decimal PesoConLaminado { get; set; }   // GM2 (PP+LAM)
    public decimal PesoConRefuerzo { get; set; }   // GMP
    public decimal PesoMetroLineal { get; set; }   // GML
    public decimal PesoPorBolsa { get; set; }       // gr/Bol

    public string ResumenFicha { get; set; } = string.Empty;
}
