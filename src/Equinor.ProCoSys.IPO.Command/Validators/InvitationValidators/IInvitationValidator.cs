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
        bool IsValidScope(IList<McPkgScopeForCommand> mcPkgScope, IList<CommPkgScopeForCommand> commPkgScope);
        bool McScopeIsUnderSameCommPkg(IList<McPkgScopeForCommand> mcPkgScope);
        Task<bool> ParticipantExistsAsync(ParticipantsForCommand participant, CancellationToken token);
        bool ParticipantMustHaveId(ParticipantsForCommand participant);
        Task<bool> ProjectNameIsNotChangedAsync(string projectName, int id, CancellationToken token);
        bool IsValidParticipantList(IList<ParticipantsForCommand> participants);
        bool RequiredParticipantsMustBeInvited(IList<ParticipantsForCommand> participants);
        bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants);
        bool NewParticipantsCannotHaveLowestSortKeys(IList<ParticipantsForCommand> participants);
    }
}
