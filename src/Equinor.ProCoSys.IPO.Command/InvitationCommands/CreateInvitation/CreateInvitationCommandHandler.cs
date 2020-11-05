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
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<int>>
    {
        private const string ContractorUserGroup = "MC_CONTRACTOR_MLA";
        private const string ConstructionUserGroup = "MC_LEAD_DISCIPLINE";

        private readonly IPlantProvider _plantProvider;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IPersonApiService _personApiService;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;

        public CreateInvitationCommandHandler(
            IPlantProvider plantProvider,
            IFusionMeetingClient meetingClient,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICommPkgApiService commPkgApiService,
            IMcPkgApiService mcPkgApiService, 
            IPersonApiService personApiService, 
            IFunctionalRoleApiService functionalRoleApiService)
        {
            _plantProvider = plantProvider;
            _meetingClient = meetingClient;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _commPkgApiService = commPkgApiService;
            _mcPkgApiService = mcPkgApiService;
            _personApiService = personApiService;
            _functionalRoleApiService = functionalRoleApiService;
        }

        public async Task<Result<int>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
        {
            var participants = new List<BuilderParticipant>();
            var invitation = new Invitation(_plantProvider.Plant, request.ProjectName, request.Title, request.Description, request.Type);
            _invitationRepository.Add(invitation);

            if (request.CommPkgScope.Count > 0)
            {
                AddCommPkgs(invitation, request.CommPkgScope, request.ProjectName);
            }

            if (request.McPkgScope.Count > 0)
            {
                AddMcPkgs(invitation, request.McPkgScope, request.ProjectName);
            }

            participants = await AddParticipantsAsync(invitation, participants, request.Participants.ToList());

            try
            {
                var meetingId = await CreateOutlookMeeting(request, participants);
                invitation.MeetingId = meetingId;
            }
            catch
            {
                return new UnexpectedResult<int>("Error: Could not create outlook meeting.");
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<int>(invitation.Id);
        }

        private async Task<List<BuilderParticipant>> AddParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> ipoParticipants)
        {
            var functionalRoleParticipants =
                ipoParticipants.Where(p => p.FunctionalRole != null).Select(p => p).ToList();
            var personsWithOids = ipoParticipants.Where(p => p.Person?.AzureOid != null).Select(p => p).ToList();
            var personParticipantsWithEmails = ipoParticipants.Where(p => p.Person != null && p.Person.AzureOid == null)
                .Select(p => p).ToList();
            var externalEmailParticipants = ipoParticipants.Where(p => p.ExternalEmail != null).Select(p => p).ToList();

            participants = functionalRoleParticipants.Count > 0
                ? await AddFunctionalRoleParticipantsAsync(invitation, participants, functionalRoleParticipants)
                : participants;
            participants = personsWithOids.Count > 0
                ? await AddPersonParticipantsWithOidsAsync(invitation, participants, personsWithOids)
                : participants;
            participants = AddExternalParticipant(invitation, participants, externalEmailParticipants);
            participants = AddPersonParticipantsWithEmails(invitation, participants, personParticipantsWithEmails);

            return participants;
        }

        private async Task<List<BuilderParticipant>> AddFunctionalRoleParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> functionalRoleParticipants)
        {
            var codes = functionalRoleParticipants.Select(p => p.FunctionalRole.Code).ToList();
            var functionalRoles =
                await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, codes);

            foreach (var participant in functionalRoleParticipants)
            {
               var fr = functionalRoles.SingleOrDefault(p => p.Code == participant.FunctionalRole.Code);
               if (fr != null)
               {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        participant.Organization,
                        IpoParticipantType.FunctionalRole,
                        fr.Code,
                        null,
                        null,
                        fr.Email,
                        null,
                        participant.SortKey));
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
                            invitation.AddParticipant(new Participant(
                                _plantProvider.Plant,
                                participant.Organization,
                                IpoParticipantType.Person,
                                fr.Code,
                                frPerson.FirstName,
                                frPerson.LastName,
                                frPerson.Email,
                                new Guid(frPerson.AzureOid),
                                participant.SortKey));
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
            }
            return participants;
        }
        
        private async Task<List<BuilderParticipant>> AddPersonParticipantsWithOidsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> personParticipantsWithOids)
        {
            if (personParticipantsWithOids.Any(p => p.SortKey == 0))
            {
                var participant = personParticipantsWithOids.Single(p => p.SortKey == 0);
                participants = await AddContractorOrConstructionCompany(
                    invitation,
                    participants,
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
                        invitation.AddParticipant(new Participant(
                            _plantProvider.Plant,
                            participant.Organization,
                            IpoParticipantType.Person,
                            null,
                            person.FirstName,
                            person.LastName,
                            person.Email,
                            new Guid(person.AzureOid),
                            participant.SortKey));
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
            PersonForCommand person,
            int sortKey,
            string userGroup)
        {
            var organization = userGroup == ConstructionUserGroup
                ? Organization.ConstructionCompany
                : Organization.Contractor;
            var p = await _personApiService.GetPersonByOidsInUserGroupAsync(_plantProvider.Plant, person.AzureOid.ToString(), userGroup);
            if (p != null)
            {
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    organization,
                    IpoParticipantType.Person,
                    null,
                    p.FirstName,
                    p.LastName,
                    p.Email,
                    new Guid(p.AzureOid),
                    sortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(new Guid(p.AzureOid))));
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
            List<ParticipantsForCommand> personsParticipantsWithEmail)
        {
            foreach (var participant in personsParticipantsWithEmail)
            {
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    participant.Organization,
                    IpoParticipantType.Person,
                    null,
                    participant.Person.FirstName, 
                    participant.Person.LastName,
                    participant.Person.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.ExternalEmail.Email)));
            }

            return participants;
        }

        private List<BuilderParticipant> AddExternalParticipant(
            Invitation invitation, 
            List<BuilderParticipant> participants, 
            List<ParticipantsForCommand> participantsWithExternalEmail)
        {
            foreach (var participant in participantsWithExternalEmail)
            {
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    participant.Organization,
                    IpoParticipantType.Person,
                    null,
                    null,
                    null,
                    participant.ExternalEmail.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.ExternalEmail.Email)));
            }
            
            return participants;
        }

        private async void AddCommPkgs(Invitation invitation, IList<string> commPkgScope, string projectName)
        {
            var commPkgDetailsList =
                await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, projectName, commPkgScope);
            
            var initialCommPkg = commPkgDetailsList.FirstOrDefault();
            if (initialCommPkg != null)
            {
                var initialSystemId = initialCommPkg.SystemId;
                if (commPkgDetailsList.Any(commPkg => commPkg.SystemId != initialSystemId))
                {
                    throw new Exception("Comm pkg scope must be within a system"); //TODO: skal vi ha exception som vises helt til brukeren?
                }
            }

            foreach (var commPkg in commPkgDetailsList)
            {
                invitation.AddCommPkg(new CommPkg(
                    _plantProvider.Plant,
                    projectName,
                    commPkg.CommPkgNo,
                    commPkg.Description,
                    commPkg.CommStatus));
            }
        }

        private async void AddMcPkgs(Invitation invitation, IList<string> mcPkgScope, string projectName)
        {
            var mcPkgDetailsList =
                await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, projectName, mcPkgScope);
            var initialMcPkg = mcPkgDetailsList.FirstOrDefault();
            if (initialMcPkg != null)
            {
                var initialCommPkgNo = initialMcPkg.CommPkgNo;
                if (mcPkgDetailsList.Any(mcPkg => mcPkg.CommPkgNo != initialCommPkgNo))
                {
                    throw new Exception("Mc pkg scope must be withing a comm pkg"); //TODO: skal vi ha exception som vises helt til brukeren?
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

        private async Task<Guid> CreateOutlookMeeting(CreateInvitationCommand request, IReadOnlyCollection<BuilderParticipant> participants)
        {
            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                meetingBuilder
                    .StandaloneMeeting(request.Title, request.Location)
                    .StartsOn(request.StartTime, request.EndTime)
                    .WithParticipants(participants)
                    .EnableOutlookIntegration(OutlookMode.All)
                    .WithClassification(MeetingClassification.Restricted)
                    .WithInviteBodyHtml(request.Description);
            });
            return meeting.Id;
        }
    }
}
