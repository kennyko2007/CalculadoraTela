namespace CalculadoraTela.Services;

/// <summary>
/// Datos para enviar correos a través de la API HTTP de Brevo
/// (https://www.brevo.com). Se usa HTTPS en vez de SMTP porque el plan
/// gratuito de Render bloquea las conexiones salientes por los puertos
/// SMTP (25, 465, 587). A diferencia de Resend, Brevo solo exige verificar
/// la dirección de correo remitente (no un dominio propio), así que se
/// puede enviar a cualquier destinatario sin comprar un dominio.
/// Los valores se cargan desde appsettings.json y/o variables de entorno
/// (ver Program.cs); la API key nunca debe quedar escrita "en duro" en el código.
/// </summary>
public class EmailSettings
{
    // Clave de API generada en Brevo (SMTP & API > API Keys).
    public string ApiKey { get; set; } = string.Empty;

    // Dirección remitente verificada en Brevo (la que confirmaste con el
    // enlace que Brevo envió al agregarla en Senders, Domains & Dedicated IPs).
    public string SenderEmail { get; set; } = string.Empty;

    // Nombre que aparece como remitente en el correo.
    public string SenderName { get; set; } = "Calculadora de Tela";
}
