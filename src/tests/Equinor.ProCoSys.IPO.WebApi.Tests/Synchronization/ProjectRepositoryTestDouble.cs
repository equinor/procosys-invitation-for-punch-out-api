using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Synchronization
{
    public class ProjectRepositoryTestDouble : IProjectRepository
    {
        private readonly List<Project> _projects = new();

        public void Add(Project item) => _projects.Add(item);

        public Task<bool> Exists(int id) => throw new NotImplementedException();

        public Task<Project> GetByIdAsync(int id) => throw new NotImplementedException();

        public Task<List<Project>> GetByIdsAsync(IEnumerable<int> id) => throw new NotImplementedException();

        public void Remove(Project entity) => throw new NotImplementedException();

        public Task<List<Project>> GetAllAsync() => throw new NotImplementedException();

        public Task<Project> GetProjectOnlyByNameAsync(string projectName) => Task.FromResult(_projects.SingleOrDefault((x => x.Name.Equals(projectName))));
    }
}
