using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken token = default);
    }
}
