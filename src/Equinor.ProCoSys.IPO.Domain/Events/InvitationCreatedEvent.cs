using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events
{
    public class InvitationCreatedEvent : INotification
    {
        public InvitationCreatedEvent(int invitationId)
        {
            InvitationId = invitationId;
        }

        public int InvitationId { get; }
    }
}
