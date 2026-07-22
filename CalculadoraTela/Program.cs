using Microsoft.EntityFrameworkCore;
// Remplaza 'CalculadoraTela.Data' por la carpeta/namespace donde esté tu DbContext
using CalculadoraTela.Data; 

var builder = WebApplication.CreateBuilder(args);

// 1. Vincular el puerto dinámico asignado por Render ($PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// 2. Agregar servicios MVC y Controllers
builder.Services.AddControllersWithViews();

// 3. Configuración del DbContext con PostgreSQL (ajusta 'CalculoContext' al nombre de tu DbContext)
builder.Services.AddDbContext<CalculoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 4. Inicialización segura de la base de datos (EnsureCreated / Migraciones)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CalculoContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al conectar con PostgreSQL.");
    }
}

// 5. Pipeline HTTP para Producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ⚠️ Quitado app.UseHttpsRedirection() para evitar redirecciones infinitas / error 502

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
