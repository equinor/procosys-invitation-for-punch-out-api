using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.MeetingCommands.CreateMeeting
{
    public class CreateMeetingCommand : IRequest<Result<Guid>>
    {
        public CreateMeetingCommand()
        {

        }
    }
}
