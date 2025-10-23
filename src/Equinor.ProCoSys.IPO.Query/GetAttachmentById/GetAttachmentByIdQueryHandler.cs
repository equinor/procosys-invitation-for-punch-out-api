using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachmentById
{
    public class GetAttachmentByIdQueryHandler(
        IReadOnlyContext context,
        IAzureBlobService blobStorage,
        IOptionsMonitor<BlobStorageOptions> blobStorageOptions,
        IQueryUserDelegationProvider queryUserDelegationProvider)
        : IRequestHandler<GetAttachmentByIdQuery, Result<AttachmentDto>>
    {
        public async Task<Result<AttachmentDto>> Handle(GetAttachmentByIdQuery request, CancellationToken cancellationToken)
        {
            var invitation = await context.QuerySet<Invitation>()
                .Include(i => i.Attachments)
                .SingleOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

            if (invitation == null)
            {
                return new NotFoundResult<AttachmentDto>(Strings.EntityNotFound(nameof(Invitation), request.InvitationId));
            }

            var attachment = invitation.Attachments.SingleOrDefault(a => a.Id == request.AttachmentId);

            if (attachment == null)
            {
                return new NotFoundResult<AttachmentDto>(Strings.EntityNotFound(nameof(Attachment), request.AttachmentId));
            }

            var uploadedBy = await context.QuerySet<Person>()
                .SingleAsync(x => x.Id == attachment.UploadedById, cancellationToken);

            return new SuccessResult<AttachmentDto>(
                new AttachmentDto(
                    attachment.Id,
                    attachment.FileName,
                    attachment.GetAttachmentDownloadUri(blobStorage, blobStorageOptions.CurrentValue, queryUserDelegationProvider),
                    attachment.UploadedAtUtc,
                    new PersonDto(
                        uploadedBy.Id,
                        uploadedBy.FirstName,
                        uploadedBy.LastName,
                        uploadedBy.UserName,
                        uploadedBy.Guid,
                        uploadedBy.Email,
                        uploadedBy.RowVersion.ConvertToString()),
                    attachment.RowVersion.ConvertToString()));
        }
    }
}
