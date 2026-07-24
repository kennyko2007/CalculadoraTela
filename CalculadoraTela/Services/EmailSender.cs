using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CalculadoraTela.Services;

/// <summary>
/// Envía correos (confirmación de cuenta, recuperación de contraseña, etc.)
/// usando el servidor SMTP de Gmail con MailKit.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpUser) || string.IsNullOrWhiteSpace(_settings.SmtpPass))
        {
            _logger.LogWarning("No hay credenciales SMTP configuradas: no se pudo enviar el correo a {Email}.", toEmail);
            throw new InvalidOperationException(
                "El servidor de correo no está configurado. Define GMAIL_USER y GMAIL_APP_PASSWORD.");
        }

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(_settings.SenderName, _settings.SmtpUser));
        mensaje.To.Add(MailboxAddress.Parse(toEmail));
        mensaje.Subject = subject;
        mensaje.Body = new TextPart("html") { Text = htmlMessage };

        using var cliente = new SmtpClient();
        try
        {
            await cliente.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await cliente.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
            await cliente.SendAsync(mensaje);
        }
        finally
        {
            if (cliente.IsConnected)
            {
                await cliente.DisconnectAsync(true);
            }
        }
    }
}
