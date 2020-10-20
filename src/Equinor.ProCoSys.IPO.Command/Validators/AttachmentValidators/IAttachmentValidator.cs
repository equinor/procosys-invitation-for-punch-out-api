using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Command.Validators.AttachmentValidators
{
    public interface IAttachmentValidator
    {
        Task<bool> ExistsAsync(int attachmentId, CancellationToken token);
    }
}
