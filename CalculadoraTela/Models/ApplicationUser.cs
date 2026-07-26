using Microsoft.AspNetCore.Identity;

namespace CalculadoraTela.Models;

/// <summary>
/// Usuario de la aplicación. Extiende IdentityUser (que ya trae UserName,
/// Email, PasswordHash, EmailConfirmed, etc.) con los campos propios que
/// necesitamos para la pantalla de registro.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
