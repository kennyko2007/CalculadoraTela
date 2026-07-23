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
    public double UrdimbreTejido { get; set; }
    public double UrdimbreDenier { get; set; }
    public double TramaTejido { get; set; }
    public double TramaDenier { get; set; }

    // Especificaciones Adicionales
    public double Laminado { get; set; }
    public double AnchoRefuerzoFactor { get; set; }
    public double Ancho { get; set; }
    public double Lado { get; set; }
    public double Corte { get; set; }
    public double Costura { get; set; }

    // Datos Producción
    public int MaquinaNumero { get; set; }
    public double Engranaje { get; set; }
    public double Horas { get; set; }

    // Resultados Calculados
    public double ResistenciaUrdimbre { get; set; }
    public double PesoUrdimbre { get; set; }
    public double PorcentajeUrdimbre { get; set; }

    public double ResistenciaTrama { get; set; }
    public double PesoTrama { get; set; }
    public double PorcentajeTrama { get; set; }

    public double PesoTejidoBase { get; set; } // G2
    public double PesoConLaminado { get; set; } // G4 (gm2)
    public double PesoConRefuerzo { get; set; } // G5 (gmp)
    public double PesoMetroLineal { get; set; } // G7 (gml)
    public double PesoPorBolsa { get; set; }   // G9 (gr/Bol)

    public double ProduccionEstimada { get; set; } // D13
    public string ResumenFicha { get; set; } = string.Empty; // A11
}
