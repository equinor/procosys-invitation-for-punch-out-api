using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

internal interface ICreateEventHelper
{
    Task<IInvitationEventV1> CreateInvitationEvent(Invitation invitation);
    Task<IParticipantEventV1> CreateParticipantEvent(Participant participant, Invitation invitation);
}
