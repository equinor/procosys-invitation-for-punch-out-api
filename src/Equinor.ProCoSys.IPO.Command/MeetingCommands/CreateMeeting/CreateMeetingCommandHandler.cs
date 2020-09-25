using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.MeetingCommands.CreateMeeting
{
    public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, Result<Guid>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IFusionMeetingClient _meetingClient;

        public CreateMeetingCommandHandler(IPlantProvider plantProvider, IFusionMeetingClient meetingClient)
        {
            _plantProvider = plantProvider;
            _meetingClient = meetingClient;
        }

        public async Task<Result<Guid>> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
        {
            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                var temp = meetingBuilder
                    .StandaloneMeeting(request.Title, request.Location)
                    .StartsOn(request.StartDate, request.EndDate)
                    .WithParticipants(request.ParticipantOids.Select(p =>
                        new BuilderParticipant(ParticipantType.Required, new ParticipantIdentifier(p))))
                    .EnableOutlookIntegration(OutlookMode.All)
                    .WithClassification(MeetingClassification.Restricted);
            });

            return new SuccessResult<Guid>(meeting.Id);
        }
    }
}
