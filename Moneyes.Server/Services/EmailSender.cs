using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Moneyes.Server.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string htmlBody = $"<html><body>{htmlMessage}</body></html>";

            MailMessage message = new
            (
                from: _mailSettings.Email,
                to: email,
                subject: subject,
                body: htmlBody
            );

            message.IsBodyHtml = true;

            SmtpClient smtpClient = new()
            {
                Host = _mailSettings.SmtpServer,
                Port = _mailSettings.SmtpPort,
                Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}
