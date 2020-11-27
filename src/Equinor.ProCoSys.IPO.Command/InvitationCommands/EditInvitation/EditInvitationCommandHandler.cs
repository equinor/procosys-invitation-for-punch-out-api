using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.Extensions.Options;
using ServiceResult;
using ParticipantType = Fusion.Integration.Meeting.ParticipantType;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommandHandler : IRequestHandler<EditInvitationCommand, Result<string>>
    {
        private const string ContractorUserGroup = "MC_CONTRACTOR_MLA";
        private const string ConstructionUserGroup = "MC_LEAD_DISCIPLINE";

        private readonly IInvitationRepository _invitationRepository;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IPersonApiService _personApiService;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;

        public EditInvitationCommandHandler(
            IInvitationRepository invitationRepository, 
            IFusionMeetingClient meetingClient,
            IPlantProvider plantProvider,
            IUnitOfWork unitOfWork,
            IMcPkgApiService mcPkgApiService,
            ICommPkgApiService commPkgApiService,
            IPersonApiService personApiService,
            IFunctionalRoleApiService functionalRoleApiService,
            IOptionsMonitor<MeetingOptions> meetingOptions)
        {
            _invitationRepository = invitationRepository;
            _meetingClient = meetingClient;
            _plantProvider = plantProvider;
            _unitOfWork = unitOfWork;
            _mcPkgApiService = mcPkgApiService;
            _commPkgApiService = commPkgApiService;
            _personApiService = personApiService;
            _functionalRoleApiService = functionalRoleApiService;
            _meetingOptions = meetingOptions;
        }

        public async Task<Result<string>> Handle(EditInvitationCommand request, CancellationToken cancellationToken)
        {
            var participants = new List<BuilderParticipant>();
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            invitation.Title = request.Title;
            invitation.Description = request.Description;
            invitation.Type = request.Type;

            UpdateMcPkgScope(invitation, request.UpdatedMcPkgScope, request.ProjectName);
            UpdateCommPkgScope(invitation, request.UpdatedCommPkgScope, request.ProjectName);

            participants = await UpdateParticipants(participants, request.UpdatedParticipants, invitation);

            try
            {
                await _meetingClient.UpdateMeetingAsync(invitation.MeetingId, builder =>
                {
                    builder.UpdateTitle(request.Title);
                    builder.UpdateLocation(request.Location);
                    builder.UpdateMeetingDate(request.StartTime, request.EndTime);
                    builder.UpdateTimeZone("UTC");
                    builder.UpdateParticipants(participants);
                    builder.UpdateInviteBodyHtml(GenerateMeetingDescription(invitation));
                });
            }
            catch
            {
                return new UnexpectedResult<string>("Error: Could not update outlook meeting.");
            }

            invitation.SetRowVersion(request.RowVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private void UpdateMcPkgScope(Invitation invitation, IList<string> mcPkgNos, string projectName)
        {
            var existingMcPkgScope = invitation.McPkgs;
            var excludedMcPkgs = existingMcPkgScope.Where(mc => !mcPkgNos.Contains(mc.McPkgNo)).ToList();
            foreach (var mcPkgToDelete in excludedMcPkgs)
            {
                invitation.RemoveMcPkg(mcPkgToDelete);
                _invitationRepository.RemoveMcPkg(mcPkgToDelete);
            }

            var existingMcPkgNos = existingMcPkgScope.Select(mc => mc.McPkgNo);
            var newMcPkgs = mcPkgNos.Where(mcPkgNo => !existingMcPkgNos.Contains(mcPkgNo)).ToList();
            if (newMcPkgs.Count > 0)
            {
                AddMcPkgs(invitation, newMcPkgs, projectName,
                    existingMcPkgScope.Count > 0 ? existingMcPkgScope.First().CommPkgNo : null);
            }
        }

        private async void AddMcPkgs(Invitation invitation, IList<string> mcPkgNos, string projectName, string commPkgNo)
        {
            var mcPkgDetailsList =
                await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, projectName, mcPkgNos);
            var initialMcPkg = mcPkgDetailsList.FirstOrDefault();
            if (initialMcPkg != null)
            {
                var initialCommPkgNo = commPkgNo ?? initialMcPkg.CommPkgNo;
                if (mcPkgDetailsList.Any(mcPkg => mcPkg.CommPkgNo != initialCommPkgNo))
                {
                    throw new Exception("Mc pkg scope must be within a comm pkg");
                }
            }
            foreach (var mcPkg in mcPkgDetailsList)
            {
                invitation.AddMcPkg(new McPkg(
                    _plantProvider.Plant,
                    projectName,
                    mcPkg.CommPkgNo,
                    mcPkg.McPkgNo,
                    mcPkg.Description));
            }
        }

        private void UpdateCommPkgScope(Invitation invitation, IList<string> commPkgNos, string projectName)
        {
            var existingCommPkgScope = invitation.CommPkgs;
            var excludedCommPkgs = existingCommPkgScope.Where(mc => !commPkgNos.Contains(mc.CommPkgNo)).ToList();
            foreach (var commPkgToDelete in excludedCommPkgs)
            {
                invitation.RemoveCommPkg(commPkgToDelete);
                _invitationRepository.RemoveCommPkg(commPkgToDelete);
            }

            var existingCommPkgs = existingCommPkgScope.Select(mc => mc.CommPkgNo).ToList();
            var newCommPkgs = commPkgNos.Where(commPkgNo => !existingCommPkgs.Contains(commPkgNo)).ToList();
            if (newCommPkgs.Count > 0)
            {
                AddCommPkgs(invitation, newCommPkgs, existingCommPkgs, projectName);
            }
        }

        private async void AddCommPkgs(Invitation invitation, IList<string> newCommPkgNos, IList<string> existingCommPkgNos, string projectName)
        {
            var commPkgDetailsList =
                await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, projectName, newCommPkgNos);

            var initialCommPkg = commPkgDetailsList.FirstOrDefault();
            if (initialCommPkg != null)
            {
                var initialSystemId = initialCommPkg.SystemId;
                if (commPkgDetailsList.Any(commPkg => commPkg.SystemId != initialSystemId))
                {
                    throw new Exception("Comm pkg scope must be within a system");
                }
            }

            foreach (var commPkg in commPkgDetailsList)
            {
                if (!existingCommPkgNos.Contains(commPkg.CommPkgNo))
                {
                    invitation.AddCommPkg(new CommPkg(
                        _plantProvider.Plant,
                        projectName,
                        commPkg.CommPkgNo,
                        commPkg.Description,
                        commPkg.CommStatus));
                }
            }
        }

        private async Task<List<BuilderParticipant>> UpdateParticipants(
            List<BuilderParticipant> participants,
            IList<ParticipantsForCommand> participantsToUpdate,
            Invitation invitation)
        {
            var existingParticipants = invitation.Participants.ToList();

            var functionalRoleParticipants =
                participantsToUpdate.Where(p => p.FunctionalRole != null).Select(p => p).ToList();
            var functionalRoleParticipantIds = functionalRoleParticipants.Select(p => p.FunctionalRole.Id).ToList();

            var personsWithOids = participantsToUpdate.Where(p => p.Person?.AzureOid != null).Select(p => p).ToList();
            var personsWithOidsIds = personsWithOids.Select(p => p.Person.Id).ToList();

            var personParticipantsWithEmails = participantsToUpdate.Where(p => p.Person != null && p.Person.AzureOid == null)
                .Select(p => p).ToList();
            var personParticipantsWithEmailsIds = personParticipantsWithEmails.Select(p => p.Person.Id).ToList();

            var externalEmailParticipants = participantsToUpdate.Where(p => p.ExternalEmail != null).Select(p => p).ToList();
            var externalEmailParticipantsIds = personParticipantsWithEmails.Select(p => p.ExternalEmail.Id).ToList();

            var participantsToUpdateIds = externalEmailParticipantsIds
                .Concat(personsWithOidsIds)
                .Concat(personParticipantsWithEmailsIds)
                .Concat(functionalRoleParticipantIds);

            var participantsToDelete = existingParticipants.Where(p => !participantsToUpdateIds.Contains(p.Id));
            foreach (var participantToDelete in participantsToDelete)
            {
                invitation.RemoveParticipant(participantToDelete);
                _invitationRepository.RemoveParticipant(participantToDelete);
            }

            participants = functionalRoleParticipants.Count > 0
                ? await UpdateFunctionalRoleParticipantsAsync(invitation, participants, functionalRoleParticipants, existingParticipants)
                : participants;
            participants = personsWithOids.Count > 0
                ? await AddPersonParticipantsWithOidsAsync(invitation, participants, personsWithOids, existingParticipants)
                : participants;
            participants = AddExternalParticipant(invitation, participants, externalEmailParticipants, existingParticipants);
            participants = AddPersonParticipantsWithEmails(invitation, participants, personParticipantsWithEmails, existingParticipants);

            return participants;
        }

        private async Task<List<BuilderParticipant>> UpdateFunctionalRoleParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IList<ParticipantsForCommand> functionalRoleParticipants,
            IList<Participant> existingParticipants)
        {
            var codes = functionalRoleParticipants.Select(p => p.FunctionalRole.Code).ToList();
            var functionalRoles =
                await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, codes);

            foreach (var participant in functionalRoleParticipants)
            {
                var fr = functionalRoles.SingleOrDefault(p => p.Code == participant.FunctionalRole.Code);
                if (fr != null)
                {
                    var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == participant.FunctionalRole.Id);
                    if (existingParticipant != null)
                    {
                        invitation.UpdateParticipant(
                            existingParticipant.Id,
                            participant.Organization,
                            IpoParticipantType.FunctionalRole,
                            fr.Code,
                            null,
                            null,
                            fr.Email,
                            null,
                            participant.SortKey,
                            participant.FunctionalRole.RowVersion);
                    }
                    else
                    {
                        invitation.AddParticipant(new Participant(
                            _plantProvider.Plant,
                            participant.Organization,
                            IpoParticipantType.FunctionalRole,
                            fr.Code,
                            null,
                            null,
                            null,
                            fr.Email,
                            null,
                            participant.SortKey));
                    }
                    
                    if (fr.UsePersonalEmail != null && fr.UsePersonalEmail == true)
                    {
                        participants.Add(new BuilderParticipant(ParticipantType.Required,
                            new ParticipantIdentifier(fr.Email)));
                    }
                    foreach (var person in participant.FunctionalRole.Persons)
                    {
                        var frPerson = fr.Persons.SingleOrDefault(p => p.AzureOid == person.AzureOid.ToString());
                        if (frPerson != null)
                        {
                            var existingPerson = existingParticipants.SingleOrDefault(p => p.Id == person.Id);
                            if (existingPerson != null)
                            {
                                invitation.UpdateParticipant(
                                    existingPerson.Id,
                                    participant.Organization,
                                    IpoParticipantType.Person,
                                    participant.FunctionalRole.Code,
                                    frPerson.FirstName,
                                    frPerson.LastName,
                                    frPerson.Email,
                                    new Guid(frPerson.AzureOid),
                                    participant.SortKey,
                                    person.RowVersion);
                            }
                            else
                            {
                                invitation.AddParticipant(new Participant(
                                    _plantProvider.Plant,
                                    participant.Organization,
                                    IpoParticipantType.Person,
                                    fr.Code,
                                    frPerson.FirstName,
                                    frPerson.LastName,
                                    frPerson.UserName,
                                    frPerson.Email,
                                    new Guid(frPerson.AzureOid),
                                    participant.SortKey));
                            }

                            if (person.Required)
                            {
                                participants.Add(new BuilderParticipant(ParticipantType.Required,
                                    new ParticipantIdentifier(new Guid(frPerson.AzureOid))));
                            }
                            else
                            {
                                participants.Add(new BuilderParticipant(ParticipantType.Optional,
                                    new ParticipantIdentifier(new Guid(frPerson.AzureOid))));
                            }
                        }
                    }
                }
                else if (participant.SortKey < 2)
                { 
                    throw new Exception($"Could not find functional role with functional role code '{participant.FunctionalRole.Code}' on required participant {participant.Organization}");
                }
            }
            return participants;
        }

        private async Task<List<BuilderParticipant>> AddPersonParticipantsWithOidsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IList<ParticipantsForCommand> personParticipantsWithOids,
            IList<Participant> existingParticipants)
        {
            if (personParticipantsWithOids.Any(p => p.SortKey == 0))
            {
                var participant = personParticipantsWithOids.Single(p => p.SortKey == 0);
                participants = await AddContractorOrConstructionCompany(
                    invitation,
                    participants,
                    existingParticipants,
                    participant.Person,
                    participant.SortKey,
                    ContractorUserGroup);
            }
            if (personParticipantsWithOids.Any(p => p.SortKey == 1))
            {
                var participant = personParticipantsWithOids.Single(p => p.SortKey == 1);
                participants = await AddContractorOrConstructionCompany(
                    invitation,
                    participants,
                    existingParticipants,
                    participant.Person,
                    participant.SortKey,
                    ConstructionUserGroup);
            }
            var oids = personParticipantsWithOids.Where(p => p.SortKey > 1).Select(p => p.Person.AzureOid.ToString()).ToList();
            var persons = oids.Count > 0
                ? await _personApiService.GetPersonsByOidsAsync(_plantProvider.Plant, oids)
                : new List<ProCoSysPerson>();
            if (persons.Any())
            {
                foreach (var participant in personParticipantsWithOids)
                {
                    var person = persons.SingleOrDefault(p => p.AzureOid == participant.Person.AzureOid.ToString());
                    if (person != null)
                    {
                        var existingParticipant =
                            existingParticipants.SingleOrDefault(p => p.Id == participant.Person.Id);
                        if (existingParticipant != null)
                        {
                            invitation.UpdateParticipant(
                                existingParticipant.Id,
                                participant.Organization,
                                IpoParticipantType.Person,
                                null,
                                person.FirstName,
                                person.LastName,
                                person.Email,
                                new Guid(person.AzureOid),
                                participant.SortKey,
                                participant.Person.RowVersion);
                        }
                        else
                        {
                            invitation.AddParticipant(new Participant(
                                _plantProvider.Plant,
                                participant.Organization,
                                IpoParticipantType.Person,
                                null,
                                person.FirstName,
                                person.LastName,
                                person.UserName,
                                person.Email,
                                new Guid(person.AzureOid),
                                participant.SortKey));
                        }
                        participants.Add(new BuilderParticipant(ParticipantType.Required,
                            new ParticipantIdentifier(new Guid(person.AzureOid))));
                    }
                }
            }

            return participants;
        }

        private async Task<List<BuilderParticipant>> AddContractorOrConstructionCompany(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IList<Participant> existingParticipants,
            PersonForCommand person,
            int sortKey,
            string userGroup)
        {
            var organization = userGroup == ConstructionUserGroup
                ? Organization.ConstructionCompany
                : Organization.Contractor;
            var personFromMain = await _personApiService.GetPersonByOidsInUserGroupAsync(_plantProvider.Plant, person.AzureOid.ToString(), userGroup);
            if (personFromMain != null)
            {
                var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == person.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        organization,
                        IpoParticipantType.Person,
                        null,
                        personFromMain.FirstName,
                        personFromMain.LastName,
                        personFromMain.Email,
                        new Guid(personFromMain.AzureOid),
                        sortKey,
                        person.RowVersion);
                }
                else
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        organization,
                        IpoParticipantType.Person,
                        null,
                        personFromMain.FirstName,
                        personFromMain.LastName,
                        personFromMain.UserName,
                        personFromMain.Email,
                        new Guid(personFromMain.AzureOid),
                        sortKey));
                }
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(new Guid(personFromMain.AzureOid))));
            }
            else
            {
                throw new Exception($"Person does not have required privileges to be the {organization} participant");
            }
            return participants;
        }

        private List<BuilderParticipant> AddPersonParticipantsWithEmails(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IEnumerable<ParticipantsForCommand> personsParticipantsWithEmail,
            IList<Participant> existingParticipants)
        {
            foreach (var participant in personsParticipantsWithEmail)
            {
                var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == participant.Person.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        participant.Person.FirstName,
                        participant.Person.LastName,
                        participant.Person.Email,
                        null,
                        participant.SortKey,
                        participant.Person.RowVersion);
                }
                else
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        participant.Person.FirstName,
                        participant.Person.LastName,
                        null,
                        participant.Person.Email,
                        null,
                        participant.SortKey));
                }
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.ExternalEmail.Email)));
            }
            return participants;
        }

        private List<BuilderParticipant> AddExternalParticipant(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IEnumerable<ParticipantsForCommand> participantsWithExternalEmail,
            IList<Participant> existingParticipants)
        {
            foreach (var participant in participantsWithExternalEmail)
            {
                var existingParticipant =
                    existingParticipants.SingleOrDefault(p => p.Id == participant.ExternalEmail.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        null,
                        null,
                        participant.ExternalEmail.Email,
                        null,
                        participant.SortKey,
                        participant.ExternalEmail.RowVersion);
                }
                else
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        null,
                        null,
                        null,
                        participant.ExternalEmail.Email,
                        null,
                        participant.SortKey));
                }
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.ExternalEmail.Email)));
            }
            return participants;
        }

        private string GenerateMeetingDescription(Invitation invitation)
        {
            var baseUrl = _meetingOptions.CurrentValue.PcsBaseUrl + _plantProvider.Plant.Substring(4, _plantProvider.Plant.Length - 4).ToUpper();
            var meetingDescription = "<h4>You have been invited to attend a punch round. The punch round will cover the following scope:</h4>";

            foreach (var mcPkg in invitation.McPkgs)
            {
                meetingDescription +=
                    $"<a href='{baseUrl}/Completion#McPkg|?projectName={invitation.ProjectName}&mcpkgno={mcPkg.McPkgNo}/'>{mcPkg.McPkgNo}</a></br>";
            }
            foreach (var commPkg in invitation.CommPkgs)
            {
                meetingDescription +=
                    $"<a href='{baseUrl}/Completion#CommPkg|?projectName={invitation.ProjectName}&commpkgno={commPkg.CommPkgNo}'>{commPkg.CommPkgNo}</a></br>";
            }

            meetingDescription += $"<p>{invitation.Description}</p>";

            meetingDescription += $"</br><a href='{baseUrl}" + $"/InvitationForPunchOut/{invitation.Id}'>" + "Open invitation for punch out in ProCoSys.</a>";

            return meetingDescription;
        }
    }
}
