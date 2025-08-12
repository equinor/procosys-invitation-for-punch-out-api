using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class InvitationHelper : IInvitationHelper
    {
        private readonly IReadOnlyContext _context;

        public InvitationHelper(IReadOnlyContext context) => _context = context;

        public async Task<string> GetProjectNameAsync(int invitationId)
        {
            var projectName = await (from p in _context.QuerySet<Project>()
                                     join i in _context.QuerySet<Invitation>() on p.Id equals i.ProjectId
                                     where i.Id == invitationId
                                     select p.Name).SingleOrDefaultAsync();

            return projectName;
        }
    }
}
