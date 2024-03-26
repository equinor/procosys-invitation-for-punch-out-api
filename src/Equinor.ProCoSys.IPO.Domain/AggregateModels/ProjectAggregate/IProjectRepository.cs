using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project> GetProjectOnlyByNameAsync(string projectName);
    Task<Project> GetProjectOnlyByProCoSysGuid(Guid proCoSysGuid);
}
