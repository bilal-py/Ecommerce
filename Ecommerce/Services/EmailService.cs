using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace Ecommerce.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body, List<string>? cc = null)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(to));

            // Add CC recipients if any
            if (cc != null && cc.Count > 0)
            {
                foreach (var ccAddress in cc)
                {
                    email.Cc.Add(MailboxAddress.Parse(ccAddress));
                }
            }

            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            //await smtp.ConnectAsync(_config["Email:SmtpHost"],
            //    int.Parse(_config["Email:Port"]),
            //    SecureSocketOptions.StartTls);

            await smtp.ConnectAsync(
                _config["Email:SmtpHost"],
                465,
                SecureSocketOptions.SslOnConnect);

            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

    }
}
