using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.MeetingCommands.CreateMeeting
{
    public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, Result<Guid>>
    {
        private readonly IPlantProvider _plantProvider;

        public CreateMeetingCommandHandler(IPlantProvider plantProvider)
        {
            _plantProvider = plantProvider;
        }

        public Task<Result<Guid>> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
