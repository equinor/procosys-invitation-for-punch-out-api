using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators
{
    public interface ISavedFilterValidator
    {
        Task<bool> ExistsWithSameTitleForPersonInProjectOrAcrossAllProjectsAsync(string title, string projectName, CancellationToken cancellationToken);
        Task<bool> ExistsAnotherWithSameTitleForPersonInProjectAsync(int savedFilterId, string title, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(int savedFilterId, CancellationToken cancellationToken);
    }
}
