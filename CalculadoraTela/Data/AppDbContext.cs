using CalculadoraTela.Models;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraTela.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Calculo> Calculos { get; set; }
}