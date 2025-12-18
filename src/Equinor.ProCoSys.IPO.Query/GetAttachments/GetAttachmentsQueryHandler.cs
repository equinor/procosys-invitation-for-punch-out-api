using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetAttachments
{
    public class GetAttachmentsQueryHandler(
        IReadOnlyContext context,
        IAzureBlobService blobStorage,
        IOptionsMonitor<BlobStorageOptions> blobStorageOptions,
        IQueryUserDelegationProvider queryUserDelegationProvider)
        : IRequestHandler<GetAttachmentsQuery, Result<List<AttachmentDto>>>
    {
        public async Task<Result<List<AttachmentDto>>> Handle(GetAttachmentsQuery request, CancellationToken cancellationToken)
        {
            // Get invitation with all attachments
            var invitation = await
                (from i in context.QuerySet<Invitation>()
                        .Include(i => i.Attachments)
                 where i.Id == request.InvitationId
                 select i).SingleOrDefaultAsync(cancellationToken);

            if (invitation == null)
            {
                return new NotFoundResult<List<AttachmentDto>>(Strings.EntityNotFound(nameof(Invitation), request.InvitationId));
            }

            var uploadedByIds = invitation.Attachments.Select(a => a.UploadedById).ToList();
            var uploadedBys = await context.QuerySet<Person>().Where(x => uploadedByIds.Contains(x.Id)).ToListAsync(cancellationToken);
            var uploadedByDtos = uploadedBys
                .Select(x =>
                    new PersonDto(
                        x.Id,
                        x.FirstName,
                        x.LastName,
                        x.UserName,
                        x.Guid,
                        x.Email,
                        x.RowVersion.ConvertToString()))
                .ToDictionary(x => x.Id);

            var attachments = invitation
                .Attachments
                .Select(attachment
                    => new AttachmentDto(
                        attachment.Id,
                        attachment.FileName,
                        attachment.GetAttachmentDownloadUri(blobStorage, blobStorageOptions.CurrentValue, queryUserDelegationProvider),
                        attachment.UploadedAtUtc,
                        uploadedByDtos[attachment.UploadedById],
                        attachment.RowVersion.ConvertToString())).ToList();

            return new SuccessResult<List<AttachmentDto>>(attachments);
        }
    }
}
