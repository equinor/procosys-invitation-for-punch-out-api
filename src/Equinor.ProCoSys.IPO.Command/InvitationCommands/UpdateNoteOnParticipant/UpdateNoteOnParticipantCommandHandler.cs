using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateNoteOnParticipant
{
    public class UpdateNoteOnParticipantCommandHandler : IRequestHandler<UpdateNoteOnParticipantCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateNoteOnParticipantCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(UpdateNoteOnParticipantCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var participant = invitation.Participants.Single(p => p.Id == request.ParticipantId);
            invitation.UpdateNote(participant, request.Note, request.RowVersion);
            //TODO: JSOI Publish new event
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(participant.RowVersion.ConvertToString());
        }
    }
}
