using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class InvitationHelper : IInvitationHelper
    {
        private readonly IReadOnlyContext _context;

        public InvitationHelper(IReadOnlyContext context) => _context = context;

        public async Task<string> GetProjectNameAsync(int invitationId)
        {
            var projectName = await (from i in _context.QuerySet<Invitation>() 
                where i.Id == invitationId
                select i.ProjectName).SingleOrDefaultAsync();
            
            return projectName;
        }
    }
}
