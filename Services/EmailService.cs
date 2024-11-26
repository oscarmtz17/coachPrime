using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Web;

public class EmailService
{
    private readonly string _senderEmail;
    private readonly string _appUrl;
    private readonly string _awsAccessKey;
    private readonly string _awsSecretKey;
    private readonly string _region;

    public EmailService(IConfiguration configuration)
    {
        _senderEmail = "support@mytracksnote.com"; // Email verificado en SES
        _appUrl = "http://localhost:3000"; // URL de tu aplicación
        _awsAccessKey = configuration["AWS:AccessKey"];
        _awsSecretKey = configuration["AWS:SecretKey"];
        _region = configuration["AWS:Region"]; // Región definida en tu configuración (us-east-2)
    }

    public async Task SendPasswordResetEmail(string recipientEmail, string resetToken)
    {
        string subject = "Recuperación de contraseña";
        // string resetLink = $"{_appUrl}/reset-password?token={resetToken}";
        // string resetLink = $"{_appUrl}/reset-password/{resetToken}";
        string encodedToken = HttpUtility.UrlEncode(resetToken);
        string resetLink = $"{_appUrl}/reset-password/{encodedToken}";


        string htmlBody = $@"
            <html>
            <head></head>
            <body>
                <h1>Recuperación de Contraseña</h1>
                <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
                <a href='{resetLink}'>Restablecer contraseña</a>
                <p>Si no solicitaste este cambio, ignora este correo.</p>
                <br />
                <p>Atentamente,</p>
                <p>Equipo de MyTracksNote</p>
            </body>
            </html>";

        string textBody = $@"
            Hemos recibido una solicitud para restablecer tu contraseña.
            Haz clic en el siguiente enlace para restablecer tu contraseña:
            {resetLink}
            Si no solicitaste este cambio, ignora este correo.
            Atentamente,
            Equipo de MyTracksNote";

        var credentials = new BasicAWSCredentials(_awsAccessKey, _awsSecretKey);

        using (var client = new AmazonSimpleEmailServiceClient(credentials, Amazon.RegionEndpoint.GetBySystemName(_region)))
        {
            var sendRequest = new SendEmailRequest
            {
                Source = _senderEmail,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { recipientEmail }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(htmlBody),
                        Text = new Content(textBody)
                    }
                }
            };

            try
            {
                var response = await client.SendEmailAsync(sendRequest);
                Console.WriteLine("Correo enviado exitosamente. Id del mensaje: " + response);
                Console.WriteLine("Correo enviado exitosamente. Id del mensaje: " + response.MessageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el correo: " + ex.Message);
                throw;
            }
        }
    }
}
