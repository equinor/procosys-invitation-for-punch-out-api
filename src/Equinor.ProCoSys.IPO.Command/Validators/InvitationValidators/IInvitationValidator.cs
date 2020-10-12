using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public interface IInvitationValidator
    {
        Task<bool> ProjectExistsAsync(string projectName, CancellationToken token);
        Task<bool> TitleExistsOnProjectAsync(string projectName, string title, CancellationToken token);
        bool RequiredParticipantsMustBeInvited(); //Contractor and construction company
        bool GuestUserMustBeValidEmailAddress(); //Handled in meeting API or here?
        bool IsValidScope(IList<McPkgScopeForCommand> mcPkgScope, IList<CommPkgScopeForCommand> commPkgScope);
    }
}
