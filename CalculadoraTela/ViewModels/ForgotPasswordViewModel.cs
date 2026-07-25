using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}
