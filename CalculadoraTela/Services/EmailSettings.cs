namespace CalculadoraTela.Services;

/// <summary>
/// Datos de la cuenta de Gmail que se usa para enviar los correos de
/// verificación. Se cargan desde appsettings.json y/o variables de entorno
/// (ver Program.cs), nunca deben quedar escritos "en duro" en el código.
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;

    // Dirección de Gmail que envía los correos (ej: micalculadora@gmail.com)
    public string SmtpUser { get; set; } = string.Empty;

    // Contraseña de aplicación de 16 caracteres generada en la cuenta de Google
    // (NO es la contraseña normal de la cuenta de Gmail).
    public string SmtpPass { get; set; } = string.Empty;

    // Nombre que aparece como remitente en el correo.
    public string SenderName { get; set; } = "Calculadora de Tela";
}
