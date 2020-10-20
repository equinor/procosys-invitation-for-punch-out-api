using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public class GetAttachmentByIdQueryHandler : IRequestHandler<GetAttachmentByIdQuery, Result<Uri>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IBlobStorage _blobStorage;
        private readonly IOptionsMonitor<BlobStorageOptions> _blobStorageOptions;

        public GetAttachmentByIdQueryHandler(IReadOnlyContext context, IBlobStorage blobStorage, IOptionsMonitor<BlobStorageOptions> blobStorageOptions)
        {
            _context = context;
            _blobStorage = blobStorage;
            _blobStorageOptions = blobStorageOptions;
        }

        public async Task<Result<Uri>> Handle(GetAttachmentByIdQuery request, CancellationToken cancellationToken)
        {
            var invitation = await _context.QuerySet<Invitation>()
                .Include(i => i.Attachments)
                .SingleOrDefaultAsync(i => i.Id == request.InvitationId);

            var attachment = invitation.Attachments.SingleOrDefault(a => a.Id == request.AttachmentId);

            if (attachment == null)
            {
                return new NotFoundResult<Uri>($"Invitation with ID {request.InvitationId} or Attachment with ID {request.AttachmentId} not found");
            }

            var now = TimeService.UtcNow;
            var fullBlobPath = Path.Combine(_blobStorageOptions.CurrentValue.BlobContainer, attachment.BlobPath).Replace("\\", "/");

            var uri = _blobStorage.GetDownloadSasUri(
                fullBlobPath,
                new DateTimeOffset(now.AddMinutes(_blobStorageOptions.CurrentValue.BlobClockSkewMinutes * -1)),
                new DateTimeOffset(now.AddMinutes(_blobStorageOptions.CurrentValue.BlobClockSkewMinutes)));
            return new SuccessResult<Uri>(uri);
        }
    }
}
