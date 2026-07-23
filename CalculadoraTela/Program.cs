using Microsoft.EntityFrameworkCore;
using Npgsql;
using CalculadoraTela.Data; // Tu namespace de AppDbContext
using CalculadoraTela.Services; // Se agrega para registrar CalculadoraService

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// Evita el error de inotify limit (file watchers) en Linux/Render
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- REGISTRO DE SERVICIOS PROPIOS DE LA APP ---
builder.Services.AddScoped<CalculadoraService>();

// --- CONFIGURACIÓN DE CADENA DE CONEXIÓN (RENDER vs LOCAL) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Si Render entrega la URL en formato postgres:// o postgresql://, la parseamos con Uri
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
        // Si la variable ya viene en formato clave=valor estándar
        connectionString = databaseUrl;
    }
}
else
{
    // Si estás ejecutando localmente en tu PC, lee appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? throw new InvalidOperationException("No se encontró 'DefaultConnection'.");
}

// Configuración de Entity Framework Core con Npgsql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// --- CREAR BASE DE DATOS Y TABLAS AL INICIAR ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); // Crea las tablas en PostgreSQL si no existen
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// --- CAMBIO PARA DETECTAR EL ERROR ---
// Forzamos el uso de la página de errores para desarrollador para ver el fallo exacto en pantalla
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
