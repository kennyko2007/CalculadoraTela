using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalculadoraTela.Models;

public class Calculo
{
    // Bolivia no tiene horario de verano (siempre UTC-4), por eso se define
    // como offset fijo en vez de depender de una zona horaria del sistema
    // operativo (que en un contenedor Docker suele venir en UTC y no tiene
    // por qué incluir la base de datos de zonas horarias IANA instalada).
    private static readonly TimeZoneInfo ZonaHorariaLocal =
        TimeZoneInfo.CreateCustomTimeZone("Bolivia", TimeSpan.FromHours(-4), "Hora de Bolivia", "Hora de Bolivia");

    [Key]
    public int Id { get; set; }

    // Se guarda siempre en UTC (buena práctica): así el dato es correcto sin
    // importar en qué zona horaria corra el servidor.
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Propiedad de solo lectura para mostrar en pantalla la hora real de
    // Bolivia, sin depender de la configuración del servidor.
    [NotMapped]
    public DateTime FechaCreacionLocal =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(FechaCreacion, DateTimeKind.Utc), ZonaHorariaLocal);

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
