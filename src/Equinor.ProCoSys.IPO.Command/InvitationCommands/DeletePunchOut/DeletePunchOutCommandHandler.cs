using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeletePunchOut
{
    public class DeletePunchOutCommandHandler : IRequestHandler<DeletePunchOutCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            IHistoryRepository historyRepository,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _historyRepository = historyRepository;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<Result<Unit>> Handle(DeletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var historyForInvitation = _historyRepository.GetHistoryBySourceGuid(invitation.Guid);
            foreach (var history in historyForInvitation)
            {
                _historyRepository.Remove(history);
            }
            invitation.SetRowVersion(request.RowVersion);
            _invitationRepository.RemoveInvitation(invitation);

            await PublishEventToBusAsync(cancellationToken, invitation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task PublishEventToBusAsync(CancellationToken cancellationToken, Invitation invitation)
        {
            var deleteInvitationMessage = new DeleteEvent
            {
                Plant = invitation.Plant, ProCoSysGuid = invitation.Guid, EntityType = nameof(Invitation)
            };

            await _integrationEventPublisher.PublishAsync(deleteInvitationMessage, cancellationToken);
        }
    }
}
