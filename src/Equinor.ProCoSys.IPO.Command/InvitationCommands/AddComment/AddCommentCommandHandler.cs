using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using System;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<int>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public AddCommentCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<Result<int>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var comment = new Comment(_plantProvider.Plant, request.Comment);
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                invitation.AddComment(comment);

                //Need to execute SaveChangesAsync to fill in values for Comment's Id, CreatedBy and CreatedAtUtc
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await PublishEventToBusAsync(cancellationToken, comment, invitation);
                //To persist to MassTransit's outbox tables
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return new SuccessResult<int>(comment.Id);
        }

        private async Task PublishEventToBusAsync(CancellationToken cancellationToken, Comment comment, Invitation invitation)
        {
            var commentEvent = _invitationRepository.GetCommentEvent(comment.Id, invitation.Id);
            await _integrationEventPublisher.PublishAsync(commentEvent, cancellationToken);
        }
    }
}
