using System;
using System.Net.Mail;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.IPO.Command.Email
{
    public interface ISmtpService
    {
        Task SendSmtpWithInviteAsync(Invitation invitation, string projectName, Person organizer, string pcsBaseUrl);
        void SendAsync(MailMessage message, string token);
    }
}
