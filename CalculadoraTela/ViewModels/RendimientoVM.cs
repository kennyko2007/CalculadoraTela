namespace CalculadoraTela.ViewModels
{
    public class RendimientoVM
    {
        public int IdMaquina { get; set; }
        public string Operario { get; set; } = string.Empty;
        public decimal ProduccionTeorica { get; set; }
        public decimal ProduccionReal { get; set; }
        public decimal PorcentajeEficiencia => ProduccionTeorica > 0 ? (ProduccionReal / ProduccionTeorica) * 100 : 0;
    }
}
