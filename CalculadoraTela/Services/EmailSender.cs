using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace CalculadoraTela.Services;

/// <summary>
/// Envía correos (confirmación de cuenta, recuperación de contraseña, etc.)
/// usando la API HTTP de Brevo (https://developers.brevo.com/reference/sendtransacemail).
/// Se usa HTTPS en vez de SMTP porque el plan gratuito de Render bloquea las
/// conexiones salientes por los puertos SMTP (25, 465, 587).
/// </summary>
public class EmailSender : IEmailSender
{
    private const string ApiUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly EmailSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> settings, HttpClient httpClient, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            _logger.LogWarning("No hay API key o remitente de Brevo configurados: no se pudo enviar el correo a {Email}.", toEmail);
            throw new InvalidOperationException(
                "El servidor de correo no está configurado. Define las variables de entorno BREVO_API_KEY y BREVO_SENDER_EMAIL.");
        }

        var payload = new
        {
            sender = new { name = _settings.SenderName, email = _settings.SenderEmail },
            to = new[] { new { email = toEmail } },
            subject,
            htmlContent = htmlMessage
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        // Brevo usa el header "api-key", no "Authorization: Bearer".
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Headers.Add("accept", "application/json");

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Brevo devolvió un error al enviar el correo a {Email}: {Status} - {Body}",
                toEmail, (int)response.StatusCode, body);
            throw new InvalidOperationException($"No se pudo enviar el correo (Brevo respondió {(int)response.StatusCode}).");
        }
    }
}
