using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachments
{
    public class GetAttachmentsQueryHandler : IRequestHandler<GetAttachmentsQuery, Result<List<AttachmentDto>>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IBlobStorage _blobStorage;
        private readonly IOptionsMonitor<BlobStorageOptions> _blobStorageOptions;

        public GetAttachmentsQueryHandler(IReadOnlyContext context, IBlobStorage blobStorage, IOptionsMonitor<BlobStorageOptions> blobStorageOptions)
        {
            _context = context;
            _blobStorage = blobStorage;
            _blobStorageOptions = blobStorageOptions;
        }

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

            var uploadedByIds = invitation.Attachments.Select(a => a.UploadedById).ToList();
            var uploadedBys = await _context.QuerySet<Person>().Where(x => uploadedByIds.Contains(x.Id)).ToListAsync(cancellationToken);
            var uploadedByDtos = uploadedBys
                .Select(x =>
                    new PersonDto(
                        x.Id,
                        x.FirstName,
                        x.LastName,
                        x.UserName,
                        x.Oid,
                        x.Email,
                        x.RowVersion.ConvertToString()))
                .ToDictionary(x => x.Id);

            var attachments = invitation
                .Attachments
                .Select(attachment
                    => new AttachmentDto(
                        attachment.Id,
                        attachment.FileName,
                        attachment.GetAttachmentDownloadUri(_blobStorage, _blobStorageOptions.CurrentValue),
                        attachment.UploadedAtUtc,
                        uploadedByDtos[attachment.UploadedById],
                        attachment.RowVersion.ConvertToString())).ToList();

            return new SuccessResult<List<AttachmentDto>>(attachments);
        }
    }
}
