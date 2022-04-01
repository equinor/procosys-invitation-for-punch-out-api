using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeletePunchOut
{
    public class DeletePunchOutCommandHandler : IRequestHandler<DeletePunchOutCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            IHistoryRepository historyRepository)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _historyRepository = historyRepository;
        }

        public async Task<Result<Unit>> Handle(DeletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var historyForInvitation = _historyRepository.GetHistoryByObjectGuid(invitation.ObjectGuid);
            foreach (var history in historyForInvitation)
            {
                _historyRepository.Remove(history);
            }
            invitation.SetRowVersion(request.RowVersion);
            _invitationRepository.RemoveInvitation(invitation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
