using Microsoft.EntityFrameworkCore;
using Npgsql;
using CalculadoraTela.Data;
using CalculadoraTela.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// Desactivar watchers en la lectura de archivos de configuración
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Registrar servicios
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CalculadoraService>();

// Cadena de conexión (Render / Local)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        var connBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Require
        };

        connectionString = connBuilder.ToString();
    }
    else
    {
        connectionString = databaseUrl;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? throw new InvalidOperationException("No se encontró 'DefaultConnection'.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// Activa los detalles de excepciones
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();

// Servir archivos estáticos de forma estándar (sin watchers adicionales)
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
