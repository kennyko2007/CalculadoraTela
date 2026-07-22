using Microsoft.EntityFrameworkCore;
using Npgsql;
using CalculadoraTela.Data;

// Inicializamos el WebApplicationBuilder deshabilitando los observadores de archivos predeterminados
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// Desactiva reloadOnChange para evitar el error de límite de inotify en Render/Linux
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.Sources.Clear();
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
          .AddEnvironmentVariables();
});

// Registrar servicios MVC
builder.Services.AddControllersWithViews();

// --- CONFIGURACIÓN DE CADENA DE CONEXIÓN (RENDER vs LOCAL) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Parsea la URL nativa de Render (postgresql://...) de forma segura sin dañar contraseñas
    var connBuilder = new NpgsqlConnectionStringBuilder(databaseUrl)
    {
        SslMode = SslMode.Prefer,
        TrustServerCertificate = true
    };

    // Asegura el puerto 5432 si la URL no lo especifica explícitamente
    if (connBuilder.Port <= 0)
    {
        connBuilder.Port = 5432;
    }

    connectionString = connBuilder.ToString();
}
else
{
    // Entorno local (PC)
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

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
