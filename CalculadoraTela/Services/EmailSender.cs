using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;

namespace CalculadoraTela.Services;

/// <summary>
/// Envía correos (confirmación de cuenta, recuperación de contraseña, etc.)
/// usando la API HTTP de Mailjet (https://dev.mailjet.com/email/reference/send-emails/).
/// Se usa HTTPS en vez de SMTP porque el plan gratuito de Render bloquea las
/// conexiones salientes por los puertos SMTP (25, 465, 587).
/// </summary>
public class EmailSender : IEmailSender
{
    private const string ApiUrl = "https://api.mailjet.com/v3.1/send";

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
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
            string.IsNullOrWhiteSpace(_settings.ApiSecret) ||
            string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            _logger.LogWarning("No hay credenciales de Mailjet configuradas: no se pudo enviar el correo a {Email}.", toEmail);
            throw new InvalidOperationException(
                "El servidor de correo no está configurado. Define las variables de entorno MAILJET_API_KEY, MAILJET_API_SECRET y MAILJET_SENDER_EMAIL.");
        }

        var payload = new
        {
            Messages = new[]
            {
                new
                {
                    From = new { Email = _settings.SenderEmail, Name = _settings.SenderName },
                    To = new[] { new { Email = toEmail } },
                    Subject = subject,
                    HTMLPart = htmlMessage
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };

        // Mailjet usa autenticación básica: API Key como usuario, Secret Key como contraseña.
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ApiKey}:{_settings.ApiSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Mailjet devolvió un error al enviar el correo a {Email}: {Status} - {Body}",
                toEmail, (int)response.StatusCode, body);
            throw new InvalidOperationException($"No se pudo enviar el correo (Mailjet respondió {(int)response.StatusCode}).");
        }
    }
}
