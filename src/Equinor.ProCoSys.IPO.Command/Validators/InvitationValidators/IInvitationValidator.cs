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
        Task<bool> ParticipantIsSignedAsync(int id, int invitationId, CancellationToken token);
        Task<bool> HasPermissionToEditParticipantAsync(int id, int invitationId, CancellationToken token);
        Task<bool> HasOppositeAttendedStatusAsync(int id, int invitationId, bool attended, CancellationToken cancellationToken);
        bool IsValidParticipantList(IList<ParticipantsForCommand> participants);
        bool RequiredParticipantsMustBeInvited(IList<ParticipantsForCommand> participants);
        bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants);
        Task<bool> AttachmentExistsAsync(int invitationId, int attachmentId, CancellationToken cancellationToken);
        Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken);
        Task<bool> IpoExistsAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoIsInStageAsync(int invitationId, IpoStatus stage, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsValidCompleterParticipantAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsValidAccepterParticipantAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoHasCompleterAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoHasAccepterAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> SignerExistsAsync(int invitationId, int participantId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsValidSigningParticipantAsync(int invitationId, int participantId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsAdminOrValidUnsigningParticipantAsync(int invitationId, int participantId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsAllowedToCancelIpoAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsAdminOrValidCompletorParticipantAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsAdminOrValidAccepterParticipantAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> CurrentUserIsAllowedToDeleteIpoAsync(int invitationId, CancellationToken cancellationToken);
    }
}
