using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Email.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly SmtpClient _client;
        private readonly ILogger _logger;

        public EmailService(IOptionsMonitor<EmailOptions> emailOptions, ILogger<EmailService> logger)
        {
            _logger = logger;
            _emailOptions = emailOptions.CurrentValue;

            if (_emailOptions.Enabled)
            {
                _client = new SmtpClient(_emailOptions.Server, _emailOptions.Port)
                {
                    EnableSsl = _emailOptions.EnableSsl,
                    Credentials = new NetworkCredential(_emailOptions.From, _emailOptions.Password)
                };
            }
        }

        public Task SendEmailsAsync(List<string> emails, string subject, string body,
            CancellationToken token = default)
        {
            if (_client != null)
            {
                var message =
                    new MailMessage(_emailOptions.From, emails[0]) { Subject = subject, Body = body, IsBodyHtml = true };

                foreach (var email in emails.Skip(1))
                {
                    message.To.Add(email);
                }

                return _client.SendMailAsync(message, token);
            }
                
            _logger.LogWarning("Email sending was requested, but service is not enabled.");
            return Task.CompletedTask;
        }
    }
}
