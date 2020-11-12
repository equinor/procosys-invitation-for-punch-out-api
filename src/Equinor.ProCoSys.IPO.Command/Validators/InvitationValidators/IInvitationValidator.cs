using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public interface IInvitationValidator
    {
        Task<bool> IpoTitleExistsInProjectAsync(string projectName, string title, CancellationToken token);
        Task<bool> IpoTitleExistsInProjectOnAnotherIpoAsync(string projectName, string title, int id, CancellationToken token);
        bool IsValidScope(IList<string> mcPkgScope, IList<string> commPkgScope);
        Task<bool> ParticipantWithIdExistsAsync(ParticipantsForCommand participant, int invitationId, CancellationToken token);
        Task<bool> ProjectNameIsNotChangedAsync(string projectName, int id, CancellationToken token);
        bool IsValidParticipantList(IList<ParticipantsForCommand> participants);
        bool RequiredParticipantsMustBeInvited(IList<ParticipantsForCommand> participants);
        bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants);
        Task<bool> AttachmentExistsAsync(int invitationId, int attachmentId, CancellationToken cancellationToken);
        Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> IpoExistsAsync(int invitationId, CancellationToken token);
        Task<bool> IpoIsInPlannedStageAsync(int invitationId, CancellationToken token);
        Task<bool> ValidContractorParticipantExistsAsync(int invitationId, CancellationToken token);
        Task<bool> ContractorExistsAsync(int invitationId, CancellationToken token);
    }
}
