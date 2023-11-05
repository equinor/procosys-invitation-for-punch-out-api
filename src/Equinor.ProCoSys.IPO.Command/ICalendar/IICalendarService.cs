using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Microsoft.Graph.Models;
using Invitation = Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation;
using Person = Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person;

namespace Equinor.ProCoSys.IPO.Command.ICalendar
{
    public interface IICalendarService
    {
        Message CreateMessage(Invitation invitation, string projectName, Person organizer, string pcsBaseUrl, CreateInvitationCommand request);
    }
}
