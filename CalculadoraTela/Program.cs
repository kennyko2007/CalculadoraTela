using Microsoft.EntityFrameworkCore;
using Npgsql;
// Reemplaza 'CalculadoraTela.Data' si tu DbContext está en otra carpeta/namespace
using CalculadoraTela.Data; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- CONFIGURACIÓN DE CADENA DE CONEXIÓN (RENDER vs LOCAL) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Si estamos en Render, parseamos la Internal Database URL (postgresql://...)
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');

    var connectionBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        // Si el puerto de la URI no es válido o es -1, forzamos el 5432 estándar de Postgres
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        Database = databaseUri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Prefer,
        TrustServerCertificate = true
    };

    connectionString = connectionBuilder.ToString();
}
else
{
    // Si estamos corriendo localmente en la PC, lee de appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? throw new InvalidOperationException("No se encontró la cadena 'DefaultConnection'.");
}

// Configuración del DbContext con PostgreSQL (Npgsql)
// NOTA: Si tu DbContext no se llama 'ApplicationDbContext', cambia el nombre aquí
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// --- APLICAR MIGRACIONES Y CREAR BASE DE DATOS AL INICIAR ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated(); // Crea las tablas automáticamente en Postgres
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// Configure the HTTP request pipeline.
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
