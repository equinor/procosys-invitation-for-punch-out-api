using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public interface IInvitationValidator
    {
        Task<bool> ExistsAsync(int invitationId, CancellationToken cancellationToken);
        Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken);
    }
}
