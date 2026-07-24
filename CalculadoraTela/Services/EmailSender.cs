using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CalculadoraTela.Services;

public class EmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var host     = _config["EmailSettings:SmtpHost"]     ?? "smtp.gmail.com";
        var port     = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
        var usuario  = _config["EmailSettings:Usuario"]      ?? throw new InvalidOperationException("EmailSettings:Usuario no configurado.");
        var password = _config["EmailSettings:Password"]     ?? throw new InvalidOperationException("EmailSettings:Password no configurado.");
        var nombre   = _config["EmailSettings:NombreRemitente"] ?? "Calculadora Tela";

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(nombre, usuario));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(usuario, password);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Correo enviado a {Destinatario}.", destinatario);
    }
}
