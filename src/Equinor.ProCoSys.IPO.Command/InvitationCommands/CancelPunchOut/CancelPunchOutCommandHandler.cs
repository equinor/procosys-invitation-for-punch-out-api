using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Fusion.Integration.Meeting;
using MediatR;
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

        public CancelPunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            IFusionMeetingClient meetingClient,
            ICurrentUserProvider currentUserProvider)
        {
            _invitationRepository = invitationRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _meetingClient = meetingClient;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<Result<string>> Handle(CancelPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            invitation.CancelIpo(currentUser);
            invitation.SetRowVersion(request.RowVersion);

            await CancelFusionMeetingAsync(invitation.MeetingId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task CancelFusionMeetingAsync(Guid meetingId)
        {
            try
            {
                await _meetingClient.DeleteMeetingAsync(meetingId);
            }
            catch (Exception e)
            {
                throw new Exception("Error: Could not cancel outlook meeting.", e);
            }
        }
    }
}
