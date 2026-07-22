// --- CONFIGURACIÓN DE CADENA DE CONEXIÓN (RENDER vs LOCAL) ---
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convertimos la URL de Render (postgresql://...) al formato que entiende Npgsql
    var builder = new NpgsqlConnectionStringBuilder(databaseUrl)
    {
        SslMode = SslMode.Prefer,
        TrustServerCertificate = true
    };

    // Aseguramos el puerto 5432 si no venía en la URL
    if (builder.Port <= 0)
    {
        builder.Port = 5432;
    }

    connectionString = builder.ToString();
}
else
{
    // Si estás en tu PC local, lee appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? throw new InvalidOperationException("No se encontró 'DefaultConnection'.");
}
