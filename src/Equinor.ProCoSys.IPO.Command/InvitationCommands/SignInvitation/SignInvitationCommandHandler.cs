using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation
{
    public class SignInvitationCommandHandler : IRequestHandler<SignInvitationCommand, Result<string>>
    {
        private const string ContractorUserGroup = "MC_CONTRACTOR_MLA";
        private const string ConstructionUserGroup = "MC_LEAD_DISCIPLINE";

        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;

        public SignInvitationCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IPersonApiService personApiService)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personApiService = personApiService;
        }

        public async Task<Result<string>> Handle(SignInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var participants = invitation.Participants.Where(p => 
                p.SortKey == 0 && 
                p.Organization == Organization.Contractor && 
                p.AzureOid == currentUserAzureOid).ToList(); //completing
            if (participants[0].FunctionalRoleCode != null)
            {
                var functionalRoleParticipant = participants.SingleOrDefault(p => p.Type == IpoParticipantType.FunctionalRole);
                await CompleteIpoAsPersonInFunctionalRoleAsync(invitation, functionalRoleParticipant);
            }
            else
            {
                var personParticipant = participants.SingleOrDefault();
                await CompleteIpoAsPersonAsync(invitation, personParticipant);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task CompleteIpoAsPersonInFunctionalRoleAsync(
            Invitation invitation, 
            Participant participant)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                participant.FunctionalRoleCode);
            //if (person != null)
            //{
            //    invitation.Status = IpoStatus.Completed;
            //    participant.SignedBy = person.UserName;
            //    participant.SignedAt = new DateTime();
            //}
        }

        private async Task CompleteIpoAsPersonAsync(
            Invitation invitation,
            Participant participant)
        {
            var person = await _personApiService.GetPersonByOidsInUserGroupAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                ContractorUserGroup);
            if (person != null)
            {
                //invitation.Status = IpoStatus.Completed;
                //participant.SignedBy = person.UserName;
                //participant.SignedAt = new DateTime();
            }
        }

    }
}
