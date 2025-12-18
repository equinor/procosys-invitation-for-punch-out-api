using System.Collections.Generic;
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

            await PublishEventToBusAsync(invitation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task PublishEventToBusAsync(Invitation invitation, CancellationToken cancellationToken)
        {
            await PublishInvitationDeleteEvent(invitation, cancellationToken);
            await PublishCommentDeleteEvents(invitation.Comments, cancellationToken);
            await PublishParticipantDeleteEvents(invitation.Participants, cancellationToken);
            await PublishMcPkgDeleteEvents(invitation.McPkgs, cancellationToken);
            await PublishCommPkgDeleteEvents(invitation.CommPkgs, cancellationToken);
        }
        private async Task PublishInvitationDeleteEvent(Invitation invitation, CancellationToken cancellationToken)
        {
            var invitationDeleteEvent = new InvitationDeleteEvent { Plant = invitation.Plant, ProCoSysGuid = invitation.Guid };
            await _integrationEventPublisher.PublishAsync(invitationDeleteEvent, cancellationToken);
        }

        private async Task PublishMcPkgDeleteEvents(IReadOnlyCollection<McPkg> mcPkgs, CancellationToken cancellationToken)
        {
            foreach (var mcPkg in mcPkgs)
            {
                var mcPkgDeleteEvent = new McPkgDeleteEvent { Plant = mcPkg.Plant, ProCoSysGuid = mcPkg.Guid };
                await _integrationEventPublisher.PublishAsync(mcPkgDeleteEvent, cancellationToken);
            }
        }

        private async Task PublishCommPkgDeleteEvents(IReadOnlyCollection<CommPkg> commPkgs, CancellationToken cancellationToken)
        {
            foreach (var commPkg in commPkgs)
            {
                var commPkgDeleteEvent = new CommPkgDeleteEvent { Plant = commPkg.Plant, ProCoSysGuid = commPkg.Guid };
                await _integrationEventPublisher.PublishAsync(commPkgDeleteEvent, cancellationToken);
            }
        }

        private async Task PublishCommentDeleteEvents(IReadOnlyCollection<Comment> comments, CancellationToken cancellationToken)
        {
            foreach (var comment in comments)
            {
                var commentDeleteEvent = new CommentDeleteEvent { Plant = comment.Plant, ProCoSysGuid = comment.Guid };
                await _integrationEventPublisher.PublishAsync(commentDeleteEvent, cancellationToken);
            }
        }

        private async Task PublishParticipantDeleteEvents(IReadOnlyCollection<Participant> participants, CancellationToken cancellationToken)
        {
            foreach (var participant in participants)
            {
                var participantDeleteEvent = new ParticipantDeleteEvent { Plant = participant.Plant, ProCoSysGuid = participant.Guid };
                await _integrationEventPublisher.PublishAsync(participantDeleteEvent, cancellationToken);
            }
        }
    }
}
