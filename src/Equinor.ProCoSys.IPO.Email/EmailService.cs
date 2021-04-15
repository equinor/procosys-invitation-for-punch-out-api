using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Email.Settings;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly SmtpClient _client;

        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
            _client = new SmtpClient(_emailOptions.Server, _emailOptions.Port)
            {
                EnableSsl = _emailOptions.EnableSsl,
                Credentials = new NetworkCredential(_emailOptions.From, _emailOptions.Password)
            };
        } 

        public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken token = default)
        {
            var message =
                new MailMessage(_emailOptions.From, toEmail) {Subject = subject, Body = body, IsBodyHtml = true};


            return _client.SendMailAsync(message, token);
        }
    }
}
