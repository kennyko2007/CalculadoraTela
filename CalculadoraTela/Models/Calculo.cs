using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.Models;

public class Calculo
{
    [Key]
    public int Id { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Nuevo campo agregado
    [Required]
    [StringLength(50)]
    public string TipoProducto { get; set; } = "Tubular";

    // Entradas Urdimbre y Trama
    public decimal UrdimbreTejido { get; set; }
    public decimal UrdimbreDenier { get; set; }
    public decimal TramaTejido { get; set; }
    public decimal TramaDenier { get; set; }

    // Especificaciones Adicionales
    public decimal Laminado { get; set; }
    public decimal AnchoRefuerzoFactor { get; set; }
    public decimal Ancho { get; set; }
    public decimal Lado { get; set; }
    public decimal Corte { get; set; }
    public decimal Costura { get; set; }

    // Datos Producción
    public int MaquinaNumero { get; set; }
    public decimal Engranaje { get; set; }
    public decimal Horas { get; set; }

    // Resultados Calculados
    public decimal ResistenciaUrdimbre { get; set; }
    public decimal PesoUrdimbre { get; set; }
    public decimal PorcentajeUrdimbre { get; set; }

    public decimal ResistenciaTrama { get; set; }
    public decimal PesoTrama { get; set; }
    public decimal PorcentajeTrama { get; set; }

    public decimal PesoTejidoBase { get; set; } // G2
    public decimal PesoConLaminado { get; set; } // G4 (gm2)
    public decimal PesoConRefuerzo { get; set; } // G5 (gmp)
    public decimal PesoMetroLineal { get; set; } // G7 (gml)
    public decimal PesoPorBolsa { get; set; }   // G9 (gr/Bol)

    public decimal ProduccionEstimada { get; set; } // D13
    public string ResumenFicha { get; set; } = string.Empty; // A11
}
