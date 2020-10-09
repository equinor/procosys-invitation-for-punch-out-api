using System.Collections.Generic;
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
            var invitation = new Invitation(_plantProvider.Plant);
            _invitationRepository.Add(invitation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var participants = new List<BuilderParticipant>();
            // By OID
            participants.AddRange(
                request.Meeting.ParticipantOids.Select(p =>
                    new BuilderParticipant(ParticipantType.Required, new ParticipantIdentifier(p))));
            // By Email
            participants.AddRange(
                request.Meeting.ParticipantEmails.Select(p =>
                    new BuilderParticipant(ParticipantType.Required, new ParticipantIdentifier(p))));

            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                meetingBuilder
                .StandaloneMeeting(request.Meeting.Title, request.Meeting.Location)
                .StartsOn(request.Meeting.StartTime, request.Meeting.EndTime)
                .WithParticipants(participants)
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
