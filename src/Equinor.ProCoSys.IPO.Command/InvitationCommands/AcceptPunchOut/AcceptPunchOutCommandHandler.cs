using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut
{
    public class AcceptPunchOutCommandHandler : IRequestHandler<AcceptPunchOutCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;
        private readonly IMcPkgApiService _mcPkgApiService;

        public AcceptPunchOutCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IPersonApiService personApiService,
            IMcPkgApiService mcPkgApiService)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personApiService = personApiService;
            _mcPkgApiService = mcPkgApiService;
        }

        public async Task<Result<string>> Handle(AcceptPunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUserAzureOid = _currentUserProvider.GetCurrentUserOid();
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 1 && 
                p.Organization == Organization.ConstructionCompany && 
                p.AzureOid == currentUserAzureOid);

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 1 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);
                try
                {
                    await AcceptIpoAsPersonInFunctionalRoleAsync(invitation, functionalRole,
                        request.ParticipantRowVersion);
                }
                catch (Exception e)
                {
                    return new UnexpectedResult<string>(e.Message);
                }
            }
            else
            {
                invitation.AcceptIpo(participant, participant.UserName, request.ParticipantRowVersion);
            }
            UpdateNotesOnParticipants(invitation, request.Participants);

            invitation.SetRowVersion(request.InvitationRowVersion);
            try
            {
                await _mcPkgApiService.SetM02DatesAsync(
                    _plantProvider.Plant,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception)
            {
                return new UnexpectedResult<string>("Error: Could not set M-02 dates");
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private void UpdateNotesOnParticipants(Invitation invitation, IList<UpdateNoteOnParticipantForCommand> participants)
        {
            foreach (var participant in participants)
            {
                var ipoParticipant = invitation.Participants.Single(p => p.Id == participant.Id);
                ipoParticipant.Note = participant.Note;
                ipoParticipant.SetRowVersion(participant.RowVersion);
            }
        }

        private async Task AcceptIpoAsPersonInFunctionalRoleAsync(
            Invitation invitation,
            Participant participant,
            string participantRowVersion)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                participant.FunctionalRoleCode);

            if (person != null)
            {
                invitation.AcceptIpo(participant, person.UserName, participantRowVersion);
            }
            else
            {
                throw new Exception($"Person was not found in functional role with code '{participant.FunctionalRoleCode}'");
            }
        }
    }
}
