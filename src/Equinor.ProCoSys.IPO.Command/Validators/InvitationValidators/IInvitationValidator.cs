using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public interface IInvitationValidator
    {
        bool IsValidScope(DisciplineType type, IList<string> mcPkgScope, IList<string> commPkgScope);
        Task<bool> ParticipantWithIdExistsAsync(ParticipantsForCommand participant, int invitationId, CancellationToken token);
        Task<bool> ParticipantExistsAsync(int id, int invitationId, CancellationToken token);
        bool IsValidParticipantList(IList<ParticipantsForCommand> participants);
        bool RequiredParticipantsMustBeInvited(IList<ParticipantsForCommand> participants);
        bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants);
        Task<bool> AttachmentExistsAsync(int invitationId, int attachmentId, CancellationToken cancellationToken);
        Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken);
        Task<bool> IpoExistsAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoIsInStageAsync(int invitationId, IpoStatus stage, CancellationToken cancellationToken);
        Task<bool> ValidCompleterParticipantExistsAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> ValidAccepterParticipantExistsAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoHasCompleterAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoHasAccepterAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> SignerExistsAsync(int invitationId, int participantId, CancellationToken cancellationToken);
        Task<bool> ValidSigningParticipantExistsAsync(int invitationId, int participantId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsCreatorOrIsInContractorFunctionalRoleOfInvitation(int invitationId, CancellationToken cancellationToken);
        Task<bool> SameUserUnCompletingThatCompletedAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> SameUserUnAcceptingThatAcceptedAsync(int invitationId, CancellationToken cancellationToken);
    }
}
