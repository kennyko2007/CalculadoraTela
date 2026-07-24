using Microsoft.AspNetCore.Identity;

namespace CalculadoraTela.Models;

public class ApplicationUser : IdentityUser
{
    public string? NombreCompleto { get; set; }
}
