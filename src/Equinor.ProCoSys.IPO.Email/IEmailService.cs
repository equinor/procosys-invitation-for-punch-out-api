using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Email
{
    public interface IEmailService
    {
        Task SendEmailsAsync(List<string> emails, string subject, string body, CancellationToken token = default);
    }
}
