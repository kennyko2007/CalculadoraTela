namespace CalculadoraTela.ViewModels
{
    public class AusenciaVM
    {
        public int Id { get; set; }
        public string Operario { get; set; } = string.Empty;
        public string Maquina { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
    }
}
