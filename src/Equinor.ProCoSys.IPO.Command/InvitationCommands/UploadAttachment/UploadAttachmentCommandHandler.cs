using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment
{
    public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, Result<int>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantProvider _plantProvider;
        private readonly IAzureBlobService _blobStorage;
        private readonly IOptionsMonitor<BlobStorageOptions> _blobStorageOptions;

        public UploadAttachmentCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            IPlantProvider plantProvider,
            IAzureBlobService blobStorage,
            IOptionsMonitor<BlobStorageOptions> blobStorageOptions)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _plantProvider = plantProvider;
            _blobStorage = blobStorage;
            _blobStorageOptions = blobStorageOptions;
        }

        public async Task<Result<int>> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var attachment = invitation.Attachments.SingleOrDefault(x => x.FileName.ToUpperInvariant() == request.FileName.ToUpperInvariant());

            if (!request.OverWriteIfExists && attachment != null)
            {
                return new InvalidResult<int>($"Invitation {invitation.Id} already has an attachment with filename {request.FileName}");
            }

            if (attachment == null)
            {
                attachment = new Attachment(_plantProvider.Plant, request.FileName);
                invitation.AddAttachment(attachment);
            }

            var fullBlobPath = attachment.GetFullBlobPath();
            await _blobStorage.UploadAsync(_blobStorageOptions.CurrentValue.BlobContainer, fullBlobPath, request.Content, request.OverWriteIfExists, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<int>(attachment.Id);
        }
    }
}
