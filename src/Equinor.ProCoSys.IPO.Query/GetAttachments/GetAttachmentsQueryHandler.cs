using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachments
{
    public class GetAttachmentsQueryHandler : IRequestHandler<GetAttachmentsQuery, Result<List<AttachmentDto>>>
    {
        private readonly IReadOnlyContext _context;

        public GetAttachmentsQueryHandler(IReadOnlyContext context) => _context = context;

        public async Task<Result<List<AttachmentDto>>> Handle(GetAttachmentsQuery request, CancellationToken cancellationToken)
        {
            // Get invitation with all attachments
            var invitation = await
                (from i in _context.QuerySet<Invitation>()
                        .Include(i => i.Attachments)
                 where i.Id == request.InvitationId
                 select i).SingleOrDefaultAsync(cancellationToken);

            if (invitation == null)
            {
                return new NotFoundResult<List<AttachmentDto>>($"Invitation with ID {request.InvitationId} not found");
            }

            var attachments = invitation
                .Attachments
                .Select(attachment
                    => new AttachmentDto(
                        attachment.Id,
                        attachment.FileName,
                        attachment.RowVersion.ConvertToString())).ToList();

            return new SuccessResult<List<AttachmentDto>>(attachments);
        }
    }
}
