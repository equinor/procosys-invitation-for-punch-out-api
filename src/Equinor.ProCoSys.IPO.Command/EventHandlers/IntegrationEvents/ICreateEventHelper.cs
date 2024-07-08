using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;

public interface ICreateEventHelper
{
    Task<IInvitationEventV1> CreateInvitationEvent(Invitation invitation);
    Task<IParticipantEventV1> CreateParticipantEvent(Participant participant, Invitation invitation);
    Task<ICommentEventV1> CreateCommentEvent(Comment comment, Invitation invitation);
    Task<ICommPkgEventV1> CreateCommPkgEvent(CommPkg commPkg, Invitation invitation);
    Task<IMcPkgEventV1> CreateMcPkgEvent(McPkg mcPkg, Invitation invitation);
}
