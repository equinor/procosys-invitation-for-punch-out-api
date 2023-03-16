using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusOnParticipant
{
    public class UpdateAttendedStatusOnParticipantCommandHandler : IRequestHandler<UpdateAttendedStatusOnParticipantCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAttendedStatusOnParticipantCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _invitationRepository = invitationRepository;
        }

        public async Task<Result<string>> Handle(UpdateAttendedStatusOnParticipantCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var participant = invitation.Participants.Single(p => p.Id == request.ParticipantId);
            invitation.UpdateAttendedStatus(participant, request.Attended, request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(participant.RowVersion.ConvertToString());
        }
    }
}
