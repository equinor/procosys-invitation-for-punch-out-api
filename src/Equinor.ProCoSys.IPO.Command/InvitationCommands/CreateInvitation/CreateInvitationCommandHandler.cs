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

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<int>>
    {
        private const string _objectName = "IPO";
        private readonly IList<string> _requiredSignerPrivileges = new List<string>{"CREATE", "SIGN"};
        private readonly IList<string> _additionalSignerPrivileges = new List<string>{"SIGN"};

        private readonly IPlantProvider _plantProvider;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IPersonApiService _personApiService;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;

        public CreateInvitationCommandHandler(
            IPlantProvider plantProvider,
            IFusionMeetingClient meetingClient,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICommPkgApiService commPkgApiService,
            IMcPkgApiService mcPkgApiService,
            IPersonApiService personApiService,
            IFunctionalRoleApiService functionalRoleApiService,
            IOptionsMonitor<MeetingOptions> meetingOptions)
        {
            _plantProvider = plantProvider;
            _meetingClient = meetingClient;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _commPkgApiService = commPkgApiService;
            _mcPkgApiService = mcPkgApiService;
            _personApiService = personApiService;
            _functionalRoleApiService = functionalRoleApiService;
            _meetingOptions = meetingOptions;
        }

        public async Task<Result<int>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _unitOfWork.BeginTransaction(cancellationToken);
            var participants = new List<BuilderParticipant>();
            var invitation = new Invitation(
                _plantProvider.Plant,
                request.ProjectName,
                request.Title,
                request.Description,
                request.Type,
                request.StartTime,
                request.EndTime,
                request.Location);
            _invitationRepository.Add(invitation);

            if (request.CommPkgScope.Count > 0)
            {
                await AddCommPkgsAsync(invitation, request.CommPkgScope, request.ProjectName);
            }

            if (request.McPkgScope.Count > 0)
            {
                await AddMcPkgsAsync(invitation, request.McPkgScope, request.ProjectName);
            }

            participants = await AddParticipantsAsync(invitation, participants, request.Participants.ToList());
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                invitation.MeetingId = await CreateOutlookMeeting(request, participants, invitation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _unitOfWork.Commit();
                return new SuccessResult<int>(invitation.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new UnexpectedResult<int>("Error: Could not create outlook meeting.");
            }
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
                        null,
                        fr.Email,
                        null,
                        participant.SortKey));
                    if (fr.UsePersonalEmail != null && fr.UsePersonalEmail == false && fr.Email != null)
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
                                frPerson.UserName,
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
               else
               {
                   throw new IpoValidationException(
                       $"Could not find functional role with functional role code '{participant.FunctionalRole.Code}' on participant {participant.Organization}.");
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
                participants = await AddSigner(
                    invitation,
                    participants,
                    participant.Person,
                    participant.SortKey,
                    Organization.Contractor,
                    _requiredSignerPrivileges);
                personParticipantsWithOids.Remove(participant);
            }
            if (personParticipantsWithOids.Any(p => p.SortKey == 1))
            {
                var participant = personParticipantsWithOids.Single(p => p.SortKey == 1);
                participants = await AddSigner(
                    invitation,
                    participants,
                    participant.Person,
                    participant.SortKey,
                    Organization.ConstructionCompany,
                    _requiredSignerPrivileges);
                personParticipantsWithOids.Remove(participant);
            }

            if (personParticipantsWithOids.Any(p =>
                p.SortKey < 5 && p.Organization == Organization.Commissioning))
            {
                var participant = personParticipantsWithOids.First(p => p.SortKey < 5 && p.Organization == Organization.Commissioning);
                participants = await AddSigner(
                    invitation,
                    participants,
                    participant.Person,
                    participant.SortKey,
                    participant.Organization,
                    _additionalSignerPrivileges);
                personParticipantsWithOids.Remove(participant);
            }

            if (personParticipantsWithOids.Any(p =>
                p.SortKey < 5 && p.Organization == Organization.TechnicalIntegrity))
            {
                var participant = personParticipantsWithOids.First(p => p.SortKey < 5 && p.Organization == Organization.TechnicalIntegrity);
                participants = await AddSigner(
                    invitation,
                    participants,
                    participant.Person,
                    participant.SortKey,
                    participant.Organization,
                    _additionalSignerPrivileges);
                personParticipantsWithOids.Remove(participant);
            }

            if (personParticipantsWithOids.Any(p =>
                p.SortKey < 5 && p.Organization == Organization.Operation))
            {
                var participant = personParticipantsWithOids.First(p => p.SortKey < 5 && p.Organization == Organization.Operation);
                participants = await AddSigner(
                    invitation,
                    participants,
                    participant.Person,
                    participant.SortKey,
                    participant.Organization,
                    _additionalSignerPrivileges);
                personParticipantsWithOids.Remove(participant);
            }

            var oids = personParticipantsWithOids.Where(p => p.SortKey > 1).Select(p => p.Person.AzureOid.ToString()).ToList();
            var persons = oids.Count > 0
                ? await _personApiService.GetPersonsByOidsAsync(_plantProvider.Plant, oids)
                : new List<ProCoSysPerson>();
            if (persons.Any())
            {
                foreach (var participant in personParticipantsWithOids.Where(p => p.SortKey > 1))
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
                            person.UserName,
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

        private async Task<List<BuilderParticipant>> AddSigner(
            Invitation invitation,
            List<BuilderParticipant> participants,
            PersonForCommand person,
            int sortKey,
            Organization organization,
            IList<string> privileges)
        {
            var p = await _personApiService.GetPersonByOidWithPrivilegesAsync(
                _plantProvider.Plant,
                person.AzureOid.ToString(),
                _objectName,
                privileges);
            if (p != null)
            {
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    organization,
                    IpoParticipantType.Person,
                    null,
                    p.FirstName,
                    p.LastName,
                    p.UserName,
                    p.Email,
                    new Guid(p.AzureOid),
                    sortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(new Guid(p.AzureOid))));
            }
            else
            {
                throw new IpoValidationException(
                    $"Person does not have required privileges to be the {organization} participant.");
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
                    null,
                    participant.Person.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.Person.Email)));
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
                    null,
                    participant.ExternalEmail.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.ExternalEmail.Email)));
            }
            
            return participants;
        }

        private async Task AddCommPkgsAsync(Invitation invitation, IList<string> commPkgScope, string projectName)
        {
            var commPkgDetailsList =
                await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, projectName, commPkgScope);
            
            var initialCommPkg = commPkgDetailsList.FirstOrDefault();
            if (initialCommPkg != null)
            {
                var initialSystemId = initialCommPkg.SystemId;
                if (commPkgDetailsList.Any(commPkg => commPkg.SystemId != initialSystemId))
                {
                    throw new IpoValidationException("Comm pkg scope must be within a system.");
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

        private async Task AddMcPkgsAsync(Invitation invitation, IList<string> mcPkgScope, string projectName)
        {
            var mcPkgDetailsList =
                await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, projectName, mcPkgScope);
            var initialMcPkg = mcPkgDetailsList.FirstOrDefault();
            if (initialMcPkg != null)
            {
                var initialCommPkgNo = initialMcPkg.CommPkgNo;
                if (mcPkgDetailsList.Any(mcPkg => mcPkg.CommPkgNo != initialCommPkgNo))
                {
                    throw new IpoValidationException("Mc pkg scope must be within a comm pkg.");
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

        private async Task<Guid> CreateOutlookMeeting(
            CreateInvitationCommand request,
            IReadOnlyCollection<BuilderParticipant> participants,
            Invitation invitation)
        {
            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                meetingBuilder
                    .StandaloneMeeting(request.Title, request.Location)
                    .StartsOn(request.StartTime, request.EndTime)
                    .WithTimeZone("UTC")
                    .WithParticipants(participants)
                    .EnableOutlookIntegration(OutlookMode.All)
                    .WithClassification(MeetingClassification.Open)
                    .WithInviteBodyHtml(GenerateMeetingDescription(invitation));
            });
            return meeting.Id;
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
