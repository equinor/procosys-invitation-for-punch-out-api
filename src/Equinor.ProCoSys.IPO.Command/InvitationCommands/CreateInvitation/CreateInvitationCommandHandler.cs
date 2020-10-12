using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<int>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateInvitationCommandHandler(
            IPlantProvider plantProvider,
            IFusionMeetingClient meetingClient,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _plantProvider = plantProvider;
            _meetingClient = meetingClient;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitation = new Invitation(_plantProvider.Plant, request.ProjectName, request.Title, request.Type);
            _invitationRepository.Add(invitation);
            if (request.CommPkgScope.Count > 0)
            {
                foreach (var commPkg in request.CommPkgScope)
                {
                    invitation.AddCommPkg(new CommPkg(_plantProvider.Plant, request.ProjectName, commPkg.CommPkgNo, commPkg.Description, commPkg.Status));
                }
            }

            if (request.McPkgScope.Count > 0)
            {
                foreach (var mcPkg in request.McPkgScope)
                {
                    invitation.AddMcPkg(new McPkg(_plantProvider.Plant, request.ProjectName, mcPkg.CommPkgNo, mcPkg.McPkgNo, mcPkg.Description));
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                meetingBuilder
                .StandaloneMeeting(request.Meeting.Title, request.Meeting.Location)
                .StartsOn(request.Meeting.StartTime, request.Meeting.EndTime)
                .WithParticipants(request.Meeting.ParticipantOids.Select(p =>
                    new BuilderParticipant(ParticipantType.Required, new ParticipantIdentifier(p))))
                .EnableOutlookIntegration(OutlookMode.All)
                .WithClassification(MeetingClassification.Restricted)
                .WithInviteBodyHtml(request.Meeting.BodyHtml);
            });
            invitation.MeetingId = meeting.Id;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult<int>(invitation.Id);
        }
    }
}
