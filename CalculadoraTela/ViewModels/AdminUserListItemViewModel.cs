namespace CalculadoraTela.ViewModels;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool EsAdministrador { get; set; }
}
