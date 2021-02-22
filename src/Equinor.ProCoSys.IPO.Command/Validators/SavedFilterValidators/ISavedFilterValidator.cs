using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators
{
    public interface ISavedFilterValidator
    {
        Task<bool> ExistsWithSameTitleForPersonInProjectAsync(string title, string projectName, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(int savedFilterId, CancellationToken token);
    }
}
