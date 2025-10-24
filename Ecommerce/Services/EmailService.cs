using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Threading;
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

            var from = _config["Email:From"];
            var host = _config["Email:SmtpHost"]; // e.g. smtp.gmail.com
            var portStr = _config["Email:Port"];
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];

            if (string.IsNullOrWhiteSpace(from))
                throw new InvalidOperationException("Email 'From' address is not configured.");

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Email SMTP host ('Email:SmtpHost') is not configured.");

            if (!int.TryParse(portStr, out var port))
            {
                // default to submission port
                port = 587;
            }

            email.From.Add(MailboxAddress.Parse(from));
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

            // Set a reasonable timeout (milliseconds) so the call doesn't hang indefinitely in deployment.
            smtp.Timeout = 60_000; //30s

            // Choose secure socket option based on port or optional config
            SecureSocketOptions secureOption;
            var secureConfig = _config["Email:SecureSocketOptions"]; // optional: "SslOnConnect" or "StartTls"
            if (!string.IsNullOrWhiteSpace(secureConfig) && Enum.TryParse<SecureSocketOptions>(secureConfig, true, out var parsed))
            {
                secureOption = parsed;
            }
            else if (port == 465)
            {
                secureOption = SecureSocketOptions.SslOnConnect;
            }
            else
            {
                secureOption = SecureSocketOptions.StartTls;
            }

            // Use a cancellation token so ConnectAsync can't hang forever
            using var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

            try
            {
                Console.WriteLine($"Attempting connection to {host}:{port} with SecureOption: {secureOption}...");
                await smtp.ConnectAsync(host, port, secureOption, connectCts.Token);

                // Authenticate only if credentials are provided
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Authenticating to SMTP server...", username);
                    await smtp.AuthenticateAsync(username, password, connectCts.Token);
                }
                Console.WriteLine("Sending email...");
                await smtp.SendAsync(email, connectCts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new InvalidOperationException("Timed out while connecting to the SMTP server. Check network access and SMTP configuration.");
            }
            catch (Exception ex)
            {
                // Surface a clearer error for deployment environment
                throw new InvalidOperationException($"Failed to send email via SMTP ({host}:{port}) - {ex.Message}", ex);
            }
            finally
            {
                try
                {
                    if (smtp.IsConnected)
                    {
                        await smtp.DisconnectAsync(true);
                    }
                }
                catch
                {
                    // ignore disconnect errors
                }
            }
        }

    }
}
