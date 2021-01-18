using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut
{
    public class CancelPunchOutCommandHandler : IRequestHandler<CancelPunchOutCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IMcPkgApiService _mcPkgApiService;

        public CancelPunchOutCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IFusionMeetingClient meetingClient,
            IMcPkgApiService mcPkgApiService)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _meetingClient = meetingClient;
            _mcPkgApiService = mcPkgApiService;
        }

        public async Task<Result<string>> Handle(CancelPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var status = invitation.Status;
            invitation.CancelIpo(currentUser);
            //TODO: her kan man klarere datoer uten å kansellere møte - worst case
            if (status == IpoStatus.Completed)
            {
                try
                {
                    await ClearM01DatesAsync(invitation);
                }
                catch (Exception e)
                {
                    return new UnexpectedResult<string>(e.Message);
                }
            }

            try
            {
                await CancelFusionMeetingAsync(invitation.MeetingId);
            }
            catch (Exception e)
            {
                return new UnexpectedResult<string>(e.Message);
            }

            invitation.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task CancelFusionMeetingAsync(Guid meetingId)
        {
            try
            {
                await _meetingClient.DeleteMeetingAsync(meetingId);
            }
            catch
            {
                throw new Exception("Error: Could not cancel outlook meeting.");
            }
        }

        private async Task ClearM01DatesAsync(Invitation invitation)
        {
            try
            {
                await _mcPkgApiService.ClearM01DatesAsync(
                    _plantProvider.Plant,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch
            {
                throw new Exception("Error: Could not set M-01 dates");
            }
        }
    }
}
