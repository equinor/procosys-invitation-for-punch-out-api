using System;
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

            await PublishEventToBusAsync(cancellationToken, invitation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }

        private async Task PublishEventToBusAsync(CancellationToken cancellationToken, Invitation invitation)
        {
            var invitationDeleteEvent = GetInvitationDeleteEvent(invitation);
            var commentDeleteEvents = GetCommentDeleteEvents(invitation.Comments);
            var participantDeleteEvents = GetParticipantDeleteEvents(invitation.Participants);

            await _integrationEventPublisher.PublishAsync(invitationDeleteEvent, cancellationToken);

            foreach (var commentDeleteEvent in commentDeleteEvents)
            {
                await _integrationEventPublisher.PublishAsync(commentDeleteEvent, cancellationToken);
            }

            foreach (var participantDeleteEvent in participantDeleteEvents)
            {
                await _integrationEventPublisher.PublishAsync(participantDeleteEvent, cancellationToken);
            }

            //TODO: JSOI Add for McPkg and CommPkg

        }

        private static InvitationDeleteEvent GetInvitationDeleteEvent(Invitation invitation) =>
            new()
            {
                Plant = invitation.Plant, ProCoSysGuid = invitation.Guid
            };

        private List<CommentDeleteEvent> GetCommentDeleteEvents(IReadOnlyCollection<Comment> comments)
        {
            var commentDeleteEvents = new List<CommentDeleteEvent>();
            foreach (var comment in comments)
            {
                commentDeleteEvents.Add(new CommentDeleteEvent {Plant = comment.Plant, ProCoSysGuid = comment.Guid});
            }
            return commentDeleteEvents;
        }

        private List<ParticipantDeleteEvent> GetParticipantDeleteEvents(IReadOnlyCollection<Participant> participants)
        {
            var participantDeleteEvents = new List<ParticipantDeleteEvent>();
            foreach (var comment in participants)
            {
                participantDeleteEvents.Add(new ParticipantDeleteEvent { Plant = comment.Plant, ProCoSysGuid = comment.Guid });
            }
            return participantDeleteEvents;
        }
    }
}
