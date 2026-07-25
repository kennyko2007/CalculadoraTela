namespace CalculadoraTela.Services;

/// <summary>
/// Datos para enviar correos a través de la API HTTP de Mailjet
/// (https://www.mailjet.com). Se usa HTTPS en vez de SMTP porque el plan
/// gratuito de Render bloquea las conexiones salientes por los puertos
/// SMTP (25, 465, 587). Mailjet solo exige verificar la dirección de
/// correo remitente por enlace (no un dominio propio ni SMS), así que se
/// puede enviar a cualquier destinatario sin comprar un dominio.
/// Los valores se cargan desde appsettings.json y/o variables de entorno
/// (ver Program.cs); las claves nunca deben quedar escritas "en duro" en el código.
/// </summary>
public class EmailSettings
{
    // Clave pública (API Key) generada en Mailjet (Account Settings > API Key Management).
    public string ApiKey { get; set; } = string.Empty;

    // Clave secreta (Secret Key) asociada a la API Key de arriba.
    public string ApiSecret { get; set; } = string.Empty;

    // Dirección remitente verificada en Mailjet (la que confirmaste con el
    // enlace que Mailjet envió al agregarla en Senders & Domains).
    public string SenderEmail { get; set; } = string.Empty;

    // Nombre que aparece como remitente en el correo.
    public string SenderName { get; set; } = "Calculadora de Tela";
}
