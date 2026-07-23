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

    [Range(0, 12, ErrorMessage = "El ancho de refuerzo debe estar entre 0 y 12.")]
    [Display(Name = "Ancho Refuerzo (cm)")]
    public decimal AnchoRefuerzoFactor { get; set; } = 0.0m; // Ajustado por defecto a 0 según rango del Excel

    // --- CONFIGURACIÓN Y ANCHO ---
    [Display(Name = "Ancho (cm)")]
    public decimal Ancho { get; set; } = 56.0m;

    [Display(Name = "Laminado")]
    public decimal Laminado { get; set; } = 0.0m; // Ajustado por defecto según Hoja 2

    [Display(Name = "Corte / Largo (cm)")]
    public decimal Corte { get; set; } = 100.0m;

    [Display(Name = "Número de Máquina")]
    public int MaquinaNumero { get; set; } = 1;

    // --- SALIDAS / RESULTADOS ---
    public decimal ResistenciaUrdimbre { get; set; }
    public decimal PesoUrdimbre { get; set; }
    public decimal PorcentajeUrdimbre { get; set; }

    public decimal ResistenciaTrama { get; set; }
    public decimal PesoTrama { get; set; }
    public decimal PorcentajeTrama { get; set; }

    public decimal UrdimbreRefuerzoResistencia { get; set; }

    public decimal PesoTejidoBase { get; set; }       // GM2 (Celda I3)
    public decimal PesoConLaminado { get; set; }    // GM2 (PP+LAM) (Celda I6)
    public decimal PesoConRefuerzo { get; set; }    // GMP (Celda I5)
    public decimal PesoMetroLineal { get; set; }    // GML (Celda I12)
    public decimal PesoPorBolsa { get; set; }         // gr/Bol

    public string ResumenFicha { get; set; } = string.Empty;
}
