using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

    // Método para enviar correo de registro
    public async Task SendRegistrationEmail(string recipientEmail, string username)
    {
        string subject = "¡Bienvenido a MyTracksNote!";
        string htmlBody = $@"
            <html>
            <head></head>
            <body>
                <h1>Bienvenido, {username}</h1>
                <p>Gracias por registrarte en MyTracksNote. Estamos emocionados de que te unas a nuestra plataforma.</p>
                <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                <br />
                <p>Atentamente,</p>
                <p>Equipo de MyTracksNote</p>
            </body>
            </html>";

        string textBody = $@"
            Bienvenido, {username}.
            Gracias por registrarte en MyTracksNote. Estamos emocionados de que te unas a nuestra plataforma.
            Si tienes alguna pregunta, no dudes en contactarnos.
            Atentamente,
            Equipo de MyTracksNote";

        await SendEmailAsync(recipientEmail, subject, htmlBody, textBody);
    }

    // Método para enviar correo de confirmación de pago
    public async Task SendPaymentConfirmationEmail(string recipientEmail, string username, string subscriptionPlan, DateTime startDate, DateTime endDate)
    {
        string subject = "Confirmación de pago exitoso";
        string htmlBody = $@"
            <html>
            <head></head>
            <body>
                <h1>¡Gracias por tu pago, {username}!</h1>
                <p>Tu suscripción al plan <strong>{subscriptionPlan}</strong> ha sido confirmada.</p>
                <p>Detalles de la suscripción:</p>
                <ul>
                    <li>Fecha de inicio: {startDate:dd/MM/yyyy}</li>
                    <li>Fecha de fin: {endDate:dd/MM/yyyy}</li>
                </ul>
                <p>Esperamos que disfrutes de nuestros servicios.</p>
                <br />
                <p>Atentamente,</p>
                <p>Equipo de MyTracksNote</p>
            </body>
            </html>";

        string textBody = $@"
            Gracias por tu pago, {username}.
            Tu suscripción al plan {subscriptionPlan} ha sido confirmada.
            Detalles de la suscripción:
            - Fecha de inicio: {startDate:dd/MM/yyyy}
            - Fecha de fin: {endDate:dd/MM/yyyy}
            Esperamos que disfrutes de nuestros servicios.
            Atentamente,
            Equipo de MyTracksNote";

        await SendEmailAsync(recipientEmail, subject, htmlBody, textBody);
    }

    // Método para enviar correo de recuperación de contraseña
    public async Task SendPasswordResetEmail(string recipientEmail, string resetToken)
    {
        string subject = "Recuperación de contraseña";
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

        await SendEmailAsync(recipientEmail, subject, htmlBody, textBody);
    }

    // Método genérico para enviar correos
    private async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, string textBody)
    {
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
