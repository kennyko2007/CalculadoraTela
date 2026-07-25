using System.ComponentModel.DataAnnotations;

namespace CalculadoraTela.ViewModels;

public class AdminEditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa el nombre completo.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Elige un nombre de usuario.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 30 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Solo letras, números, puntos, guiones y guion bajo.")]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa un correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Dar permisos de administrador")]
    public bool EsAdministrador { get; set; }
}
