using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeletePunchOut
{
    public class DeletePunchOutCommandHandler : IRequestHandler<DeletePunchOutCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReadOnlyContext _context;


        public DeletePunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork, IHistoryRepository historyRepository, IReadOnlyContext context)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _historyRepository = historyRepository;
            _context = context;
        }

        public async Task<Result<Unit>> Handle(DeletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            IList<History> historyForInvitation = await (from history in _context.QuerySet<History>()
                where EF.Property<Guid>(history, "ObjectGuid") == invitation.ObjectGuid
                select history).ToListAsync(cancellationToken);
            foreach (var history in historyForInvitation)
            {
                _historyRepository.Remove(history);
            }
            _invitationRepository.RemoveInvitation(invitation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
