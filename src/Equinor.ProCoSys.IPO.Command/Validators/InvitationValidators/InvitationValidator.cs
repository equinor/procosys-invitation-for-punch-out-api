using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public class InvitationValidator : IInvitationValidator
    {
        private readonly IReadOnlyContext _context;

        public InvitationValidator(IReadOnlyContext context) => _context = context;

        public async Task<bool> ProjectExistsAsync(string projectName, CancellationToken token)
        {
            //TODO: check if project exists
            return true;
        }

        public bool IsValidScope(
            IList<McPkgScopeForCommand> mcPkgScope,
            IList<CommPkgScopeForCommand> commPkgScope) 
                => (mcPkgScope.Count > 0 || commPkgScope.Count > 0) && (mcPkgScope.Count < 1 || commPkgScope.Count < 1);

        public async Task<bool> TitleExistsOnProjectAsync(string projectName, string title, CancellationToken token)
        => await(from invitation in _context.QuerySet<Invitation>()
                where invitation.Title == title && invitation.ProjectName == projectName
                select invitation).AnyAsync(token);

        public bool RequiredParticipantsMustBeInvited() => true; //Todo: Contractor and construction company
        public bool GuestUserMustBeValidEmailAddress() => true; //Todo: Handled in meeting API or here?
    }
}
