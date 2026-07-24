using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using CalculadoraTela.Data;
using CalculadoraTela.Models;
using CalculadoraTela.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// --- CONFIGURACIÓN DE PUERTO PARA RENDER ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

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
    options.UseNpgsql(connectionString)
          .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// --- LOGIN / IDENTITY ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Exige confirmar el correo antes de poder iniciar sesión.
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;

    // Reglas de contraseña razonables (ajústalas si quieres ser más estricto).
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // Nombre de usuario único, sin exigir que el correo también lo sea como usuario.
    options.User.RequireUniqueEmail = true;

    // Bloqueo temporal tras varios intentos fallidos.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Rutas a las que redirige el login por cookie cuando no hay sesión o no hay permiso.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// --- CORREO (API HTTP de Brevo, no SMTP) ---
// El plan gratuito de Render bloquea las conexiones salientes por los
// puertos SMTP, así que el correo se envía por HTTPS contra la API de Brevo.
// A diferencia de Resend, Brevo solo exige verificar la dirección de correo
// remitente (no un dominio propio), así que se puede enviar a cualquier
// usuario sin comprar un dominio.
// Credenciales por variables de entorno (recomendado en Render) con
// respaldo en appsettings.json para desarrollo local.
builder.Services.Configure<EmailSettings>(options =>
{
    builder.Configuration.GetSection("EmailSettings").Bind(options);

    var envApiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");
    var envSenderEmail = Environment.GetEnvironmentVariable("BREVO_SENDER_EMAIL");

    if (!string.IsNullOrEmpty(envApiKey)) options.ApiKey = envApiKey;
    if (!string.IsNullOrEmpty(envSenderEmail)) options.SenderEmail = envSenderEmail;
});
builder.Services.AddHttpClient<IEmailSender, EmailSender>();

var app = builder.Build();

// Inicializar y actualizar base de datos mediante migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos.");
    }
}

// Activa los detalles de excepciones
app.UseDeveloperExceptionPage();

// No usamos UseHttpsRedirection() porque Render ya fuerza HTTPS en su
// proxy/edge antes de que la petición llegue al contenedor (que solo
// recibe HTTP puro). Dejarlo activo generaba el warning "Failed to
// determine the https port for redirect." sin aportar nada, porque el
// usuario ya siempre entra por https://calculadoratela.onrender.com.

// Servir archivos estáticos de forma estándar (sin watchers adicionales)
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
