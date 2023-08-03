using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class ProjectRepository : RepositoryBase<Project>, IProjectRepository
    {
        public ProjectRepository(IPOContext context)
            : base(context, context.Projects,
                context.Projects)
            
        {
        }

        public Task<Project> GetProjectOnlyByNameAsync(string projectName)
            => Set.SingleOrDefaultAsync(p => p.Name == projectName);
    }
}
