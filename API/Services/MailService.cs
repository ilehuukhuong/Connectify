using API.Interfaces;
using API.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace API.Services
{
    public class MailService : IMailService
    {
        private readonly OutlookMailSettings _mailSettings;

        public MailService(IOptions<OutlookMailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Connectify", _mailSettings.UserName));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}