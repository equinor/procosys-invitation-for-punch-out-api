using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut
{
    public class CompletePunchOutCommandHandler : IRequestHandler<CompletePunchOutCommand, Result<string>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonApiService _personApiService;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IPersonRepository _personRepository;

        public CompletePunchOutCommandHandler(
            IPlantProvider plantProvider,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider, 
            IPersonApiService personApiService,
            IMcPkgApiService mcPkgApiService,
            IPersonRepository personRepository)
        {
            _plantProvider = plantProvider;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personApiService = personApiService;
            _mcPkgApiService = mcPkgApiService;
            _personRepository = personRepository;
        }

        public async Task<Result<string>> Handle(CompletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var completedAtUtc = DateTime.UtcNow;
            var participant = invitation.Participants.SingleOrDefault(p => 
                p.SortKey == 0 && 
                p.Organization == Organization.Contractor && 
                p.AzureOid == currentUser.Oid);

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 0 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);

                await CompleteIpoAsPersonInFunctionalRoleAsync(
                    invitation,
                    functionalRole,
                    currentUser,
                    completedAtUtc,
                    request.ParticipantRowVersion);
            }
            else
            {
                invitation.CompleteIpo(participant, request.ParticipantRowVersion, currentUser, completedAtUtc);
            }
            UpdateAttendedStatusAndNotesOnParticipants(invitation, request.Participants);
            invitation.SetRowVersion(request.InvitationRowVersion);

            try
            {
                await _mcPkgApiService.SetM01DatesAsync(
                    _plantProvider.Plant,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception)
            {
                return new UnexpectedResult<string>("Error: Could not set M-01 dates");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private void UpdateAttendedStatusAndNotesOnParticipants(Invitation invitation,
            IList<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            foreach (var participant in participants)
            {
                var ipoParticipant = invitation.Participants.Single(p => p.Id == participant.Id);
                ipoParticipant.Note = participant.Note;
                ipoParticipant.Attended = participant.Attended;
                ipoParticipant.SetRowVersion(participant.RowVersion);
            }
        }

        private async Task CompleteIpoAsPersonInFunctionalRoleAsync(
            Invitation invitation,
            Participant participant,
            Person currentUser,
            DateTime completedAtUtc,
            string participantRowVersion)
        {
            var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                _plantProvider.Plant,
                _currentUserProvider.GetCurrentUserOid().ToString(),
                participant.FunctionalRoleCode);

            if (person != null)
            {
                invitation.CompleteIpo(participant, participantRowVersion, currentUser, completedAtUtc);
            }
            else
            {
                throw new Exception($"Person was not found in functional role with code '{participant.FunctionalRoleCode}'");
            }
        }
    }
}
