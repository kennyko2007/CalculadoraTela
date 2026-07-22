using Microsoft.EntityFrameworkCore;
using Npgsql;
using CalculadoraTela.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar el puerto para Render (0.0.0.0 en el $PORT dinámico)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Registrar servicios MVC
builder.Services.AddControllersWithViews();

// --- CONFIGURACIÓN DE CADENA DE CONEXIÓN (RENDER vs LOCAL) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Si la cadena viene en formato URL (postgresql://user:pass@host:port/db)
    if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        var connBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : "",
            Database = databaseUri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Prefer
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
    // Entorno local en Visual Studio
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? throw new InvalidOperationException("No se encontró 'DefaultConnection'.");
}

// Configurar DbContext con PostgreSQL
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
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// ⚠️ Muestra la página detallada con el error exacto (Línea, variable o tabla)
app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
