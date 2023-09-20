using System;
using System.Net.Mail;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;

namespace Equinor.ProCoSys.IPO.Command.Email
{
    public interface ISmtpService
    {
        Task SendSmtpWithInviteAsync(Invitation invitation, string projectName, Person organizer, string pcsBaseUrl, CreateInvitationCommand request);
        Task SendAsync(MailMessage message);
    }
}
