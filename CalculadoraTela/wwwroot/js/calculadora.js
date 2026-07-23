namespace CalculadoraTela.Models;

public class Calculo
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string TipoProducto { get; set; } = string.Empty;

    public decimal UrdimbreTejido { get; set; }
    public decimal UrdimbreDenier { get; set; }
    public decimal TramaTejido { get; set; }
    public decimal TramaDenier { get; set; }
    public decimal Laminado { get; set; }
    public decimal AnchoRefuerzoFactor { get; set; }
    public decimal Ancho { get; set; }
    public decimal Corte { get; set; }
    public int MaquinaNumero { get; set; }

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
