using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project> GetProjectOnlyByNameAsync(string projectName);
}
