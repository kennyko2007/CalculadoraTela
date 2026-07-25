using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre de usuario.")]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener sesión iniciada")]
    public bool RecordarSesion { get; set; } = true;

    public string? ReturnUrl { get; set; }
}
