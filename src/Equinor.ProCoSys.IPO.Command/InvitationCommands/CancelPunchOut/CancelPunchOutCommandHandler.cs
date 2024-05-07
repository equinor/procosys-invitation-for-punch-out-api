using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut
{
    public class CancelPunchOutCommandHandler : IRequestHandler<CancelPunchOutCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly ILogger<CancelPunchOutCommandHandler> _logger;

        public CancelPunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            IFusionMeetingClient meetingClient,
            ICurrentUserProvider currentUserProvider,
            IIntegrationEventPublisher integrationEventPublisher,
            ILogger<CancelPunchOutCommandHandler> logger)
        {
            _invitationRepository = invitationRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _meetingClient = meetingClient;
            _currentUserProvider = currentUserProvider;
            _integrationEventPublisher = integrationEventPublisher;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(CancelPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            invitation.CancelIpo(currentUser);
            invitation.SetRowVersion(request.RowVersion);

            await CancelFusionMeetingAsync(invitation.MeetingId);

            await PublishEventToBusAsync(cancellationToken, invitation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task PublishEventToBusAsync(CancellationToken cancellationToken, Invitation invitation)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = invitation.Plant,
                Event = "Canceled",
                InvitationGuid = invitation.Guid,
                IpoStatus = invitation.Status
            };

            await _integrationEventPublisher.PublishAsync(eventMessage, cancellationToken);
        }

        private async Task CancelFusionMeetingAsync(Guid meetingId)
        {
            try
            {
                await _meetingClient.DeleteMeetingAsync(meetingId);
            }
            catch (MeetingApiException e)
            {
                if (e.Code.ToString().ToUpperInvariant() == "FORBIDDEN")
                {
                    _logger.LogError(e, $"Unable to cancel outlook meeting for IPO.");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
