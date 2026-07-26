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
    // La verificación de correo al registrarse está desactivada: los
    // usuarios pueden iniciar sesión de inmediato tras crear su cuenta.
    // El correo se sigue usando solo para "Olvidé mi contraseña".
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;

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

    // Sesión de larga duración: el usuario sigue conectado mientras siga
    // usando la app (SlidingExpiration renueva la cookie en cada visita).
    // Solo se cierra si expira por inactividad prolongada (1 año) o si el
    // administrador elimina al usuario / le fuerza el cierre de sesión
    // (Identity revalida el "security stamp" contra la base cada 30 min).
    options.ExpireTimeSpan = TimeSpan.FromDays(365);
    options.SlidingExpiration = true;
});

// --- CORREO (API HTTP de Mailjet, no SMTP) ---
// El plan gratuito de Render bloquea las conexiones salientes por los
// puertos SMTP, así que el correo se envía por HTTPS contra la API de Mailjet.
// Mailjet solo exige verificar la dirección de correo remitente por enlace
// (no un dominio propio ni SMS), así que se puede enviar a cualquier usuario
// sin comprar un dominio.
// Credenciales por variables de entorno (recomendado en Render) con
// respaldo en appsettings.json para desarrollo local.
builder.Services.Configure<EmailSettings>(options =>
{
    builder.Configuration.GetSection("EmailSettings").Bind(options);

    var envApiKey = Environment.GetEnvironmentVariable("MAILJET_API_KEY");
    var envApiSecret = Environment.GetEnvironmentVariable("MAILJET_API_SECRET");
    var envSenderEmail = Environment.GetEnvironmentVariable("MAILJET_SENDER_EMAIL");

    if (!string.IsNullOrEmpty(envApiKey)) options.ApiKey = envApiKey;
    if (!string.IsNullOrEmpty(envApiSecret)) options.ApiSecret = envApiSecret;
    if (!string.IsNullOrEmpty(envSenderEmail)) options.SenderEmail = envSenderEmail;
});
builder.Services.AddHttpClient<IEmailSender, EmailSender>();

var app = builder.Build();

// Inicializar y actualizar base de datos mediante migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        // Este proyecto no tiene carpeta de migraciones de EF Core (Migrate()
        // no encuentra ninguna), así que los cambios de esquema se aplican
        // aquí con SQL directo, de forma segura para volver a ejecutarse en
        // cada arranque (IF NOT EXISTS).
        context.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Calculos\" ADD COLUMN IF NOT EXISTS \"UserId\" character varying(450) NULL;");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos.");
    }

    // --- SEMBRAR ROL Y USUARIO ADMINISTRADOR ---
    // El administrador puede crear y eliminar usuarios desde /Admin, sin
    // pasar por el registro público. Se crea (o actualiza) automáticamente
    // en cada arranque a partir de variables de entorno, para no dejar
    // credenciales fijas en el código.
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        const string rolAdmin = "Admin";
        if (!await roleManager.RoleExistsAsync(rolAdmin))
        {
            await roleManager.CreateAsync(new IdentityRole(rolAdmin));
        }

        var adminUserName = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@calculadoratela.local";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        var adminExistente = await userManager.FindByNameAsync(adminUserName);
        if (adminExistente == null)
        {
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                // Contraseña temporal solo para no dejar la cuenta sin crear;
                // cámbiala de inmediato desde "Cambiar contraseña" o define
                // ADMIN_PASSWORD como variable de entorno en Render.
                adminPassword = "CambiaEstaClave.2026";
                logger.LogWarning(
                    "No se definió ADMIN_PASSWORD: se creó el usuario administrador '{Usuario}' con una contraseña temporal. " +
                    "Cámbiala de inmediato o define ADMIN_PASSWORD en las variables de entorno.", adminUserName);
            }

            var nuevoAdmin = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true,
                NombreCompleto = "Administrador",
                FechaRegistro = DateTime.UtcNow
            };

            var resultadoCreacion = await userManager.CreateAsync(nuevoAdmin, adminPassword);
            if (resultadoCreacion.Succeeded)
            {
                await userManager.AddToRoleAsync(nuevoAdmin, rolAdmin);
                logger.LogInformation("Usuario administrador '{Usuario}' creado correctamente.", adminUserName);
            }
            else
            {
                logger.LogError("No se pudo crear el usuario administrador: {Errores}",
                    string.Join(", ", resultadoCreacion.Errors.Select(e => e.Description)));
            }
        }
        else if (!await userManager.IsInRoleAsync(adminExistente, rolAdmin))
        {
            await userManager.AddToRoleAsync(adminExistente, rolAdmin);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al sembrar el rol/usuario administrador.");
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
