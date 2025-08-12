using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment
{
    public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAzureBlobService _blobStorage;
        private readonly IOptionsMonitor<BlobStorageOptions> _blobStorageOptions;

        public DeleteAttachmentCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            IAzureBlobService blobStorage,
            IOptionsMonitor<BlobStorageOptions> blobStorageOptions)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _blobStorage = blobStorage;
            _blobStorageOptions = blobStorageOptions;
        }

        public async Task<Result<Unit>> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);

            var attachment = invitation.Attachments.Single(a => a.Id == request.AttachmentId);
            attachment.SetRowVersion(request.RowVersion);

            var fullBlobPath = attachment.GetFullBlobPath();
            await _blobStorage.DeleteAsync(_blobStorageOptions.CurrentValue.BlobContainer, fullBlobPath, cancellationToken);

            invitation.RemoveAttachment(attachment);
            _invitationRepository.RemoveAttachment(attachment);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
