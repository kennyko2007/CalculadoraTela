using Microsoft.EntityFrameworkCore;
using Npgsql;
using CalculadoraTela.Data;
using CalculadoraTela.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// 1. SOLUCIÓN AL ERROR DE INOTIFY EN LINUX/RENDER: Usar polling para el watcher de archivos
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "1");

var builder = WebApplication.CreateBuilder(args);

// Desactiva reloadOnChange para evitar que intente abrir observadores de archivos en Linux
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.Sources.Clear();
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
          .AddEnvironmentVariables();
});

// Configurar el puerto dinámico para Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Registrar servicios MVC
builder.Services.AddControllersWithViews();

// Registrar servicio de la calculadora
builder.Services.AddScoped<CalculadoraService>();

// --- CONFIGURACIÓN DE CADENA DE CONEXIÓN (RENDER vs LOCAL) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
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

app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
