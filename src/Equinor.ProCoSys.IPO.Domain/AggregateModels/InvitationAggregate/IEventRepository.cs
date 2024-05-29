using System;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
public interface IEventRepository
{
    IInvitationEventV1 GetInvitationEvent(Guid invitationGuid);
    ICommentEventV1 GetCommentEvent(Guid invitationGuid, Guid commentGuid);
    IParticipantEventV1 GetParticipantEvent(Guid invitationGuid, Guid participantGuid);
    Invitation GetInvitationFromLocal(Guid invitationGuid);
    IMcPkgEventV1 GetMcPkgEvent(Guid invitationGuid, Guid mcPkgGuid);
    ICommPkgEventV1 GetCommPkgEvent(Guid invitationGuid, Guid commPkgGuid);
}
