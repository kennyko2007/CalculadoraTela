using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalculadoraTela.Models;

public class Calculo
{
    [Key]
    public int Id { get; set; }

    // Se guarda siempre en UTC (buena práctica para BD). La hora "real"
    // que ve el usuario se calcula en FechaCreacionLocal más abajo.
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Hora real de Bolivia (UTC-4, sin horario de verano) para mostrar en
    // las vistas. Se calcula a partir de FechaCreacion (UTC) y no depende
    // de la zona horaria configurada en el servidor/contenedor donde
    // corre la app, que en Render suele ser UTC y hacía que la fecha
    // mostrada no coincidiera con la hora real.
    [NotMapped]
    public DateTime FechaCreacionLocal => FechaCreacion.AddHours(-4);

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
    public decimal Corte { get; set; }

    // Datos Producción
    public int MaquinaNumero { get; set; }

    // Resultados Calculados
    public decimal ResistenciaUrdimbre { get; set; }
    public decimal PesoUrdimbre { get; set; }
    public decimal PorcentajeUrdimbre { get; set; }

    public decimal ResistenciaTrama { get; set; }
    public decimal PesoTrama { get; set; }
    public decimal PorcentajeTrama { get; set; }
    
    public decimal UrdimbreRefuerzoResistencia { get; set; }

    public decimal PesoTejidoBase { get; set; } 
    public decimal PesoConLaminado { get; set; } 
    public decimal PesoConRefuerzo { get; set; } 
    public decimal PesoMetroLineal { get; set; } 
    public decimal PesoPorBolsa { get; set; }   

    public string ResumenFicha { get; set; } = string.Empty; 
}
