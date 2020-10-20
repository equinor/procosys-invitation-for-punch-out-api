using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public class InvitationValidator : IInvitationValidator
    {
        private readonly IReadOnlyContext _context;

        public InvitationValidator(IReadOnlyContext context)
        {
            _context = context;
        }

        public async Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken)
        {
            var invitation = await GetInvitationWithAttachments(invitationId, cancellationToken);
            return invitation?.Attachments.SingleOrDefault(a => a.FileName.ToUpperInvariant() == fileName.ToUpperInvariant()) != null;
        }

        public async Task<bool> ExistsAsync(int invitationId, CancellationToken token) =>
            await (from i in _context.QuerySet<Invitation>()
                   where i.Id == invitationId
                   select i).AnyAsync(token);

        private async Task<Invitation> GetInvitationWithAttachments(int invitationId, CancellationToken cancellationToken)
        {
            var invitation = await (from i in _context.QuerySet<Invitation>().Include(i => i.Attachments)
                                    where i.Id == invitationId
                                    select i).SingleOrDefaultAsync(cancellationToken);
            return invitation;
        }
    }
}
