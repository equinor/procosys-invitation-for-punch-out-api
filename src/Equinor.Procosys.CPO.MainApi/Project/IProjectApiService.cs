using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Project
{
    public interface IProjectApiService
    {
        Task<ProcosysProject> TryGetProjectAsync(string plant, string name);
    }
}
