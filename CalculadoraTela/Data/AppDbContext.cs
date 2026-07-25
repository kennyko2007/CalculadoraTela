using CalculadoraTela.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraTela.Data;

// Heredamos de IdentityDbContext<ApplicationUser> para que EF Core cree y
// administre automáticamente las tablas AspNetUsers, AspNetRoles,
// AspNetUserRoles, AspNetUserClaims, AspNetUserLogins, AspNetUserTokens y
// AspNetRoleClaims (las mismas que ya se ven en la base de datos).
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Calculo> Calculos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Nombres de tabla explícitos (coinciden con lo que ya existe en la
        // base de datos, en PascalCase, como el resto del proyecto).
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
    }
}
