using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Command.Validators.AttachmentValidators
{
    public class AttachmentValidator : IAttachmentValidator
    {
        private readonly IReadOnlyContext _context;

        public AttachmentValidator(IReadOnlyContext context) => _context = context;

        public async Task<bool> ExistsAsync(int attachmentId, CancellationToken token) =>
            await (from a in _context.QuerySet<Attachment>()
                   where a.Id == attachmentId
                   select a).AnyAsync(token);
    }
}
