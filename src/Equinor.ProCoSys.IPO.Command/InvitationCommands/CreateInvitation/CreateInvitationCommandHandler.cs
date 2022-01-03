using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
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
        private readonly IList<string> _signerPrivileges = new List<string> { "SIGN" };
        private readonly ILogger<CreateInvitationCommandHandler> _logger;

        private readonly IPlantProvider _plantProvider;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IPersonApiService _personApiService;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUserProvider _currentUserProvider;

        public CreateInvitationCommandHandler(
            IPlantProvider plantProvider,
            IFusionMeetingClient meetingClient,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICommPkgApiService commPkgApiService,
            IMcPkgApiService mcPkgApiService,
            IPersonApiService personApiService,
            IFunctionalRoleApiService functionalRoleApiService,
            IOptionsMonitor<MeetingOptions> meetingOptions,
            IPersonRepository personRepository,
            ICurrentUserProvider currentUserProvider,
            ILogger<CreateInvitationCommandHandler> logger)
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
            _personRepository = personRepository;
            _currentUserProvider = currentUserProvider;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _unitOfWork.BeginTransaction(cancellationToken);
            var participants = new List<BuilderParticipant>();
            var mcPkgs = new List<McPkg>();
            var commPkgs = new List<CommPkg>();

            if (request.CommPkgScope.Count > 0)
            {
                commPkgs = await GetCommPkgsToAddAsync(request.CommPkgScope, request.ProjectName);
            }

            if (request.McPkgScope.Count > 0)
            {
                mcPkgs = await GetMcPkgsToAddAsync(request.McPkgScope, request.ProjectName);
            }

            var invitation = new Invitation(
                _plantProvider.Plant,
                request.ProjectName,
                request.Title,
                request.Description,
                request.Type,
                request.StartTime,
                request.EndTime,
                request.Location,
                mcPkgs,
                commPkgs);
            _invitationRepository.Add(invitation);

            participants = await AddParticipantsAsync(invitation, participants, request.Participants.ToList());
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                invitation.MeetingId = await CreateOutlookMeeting(request, participants, invitation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _unitOfWork.Commit();
                return new SuccessResult<int>(invitation.Id);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception($"Error: User with oid {_currentUserProvider.GetCurrentUserOid()} could not create outlook meeting for invitation {invitation.Id}.", e);
            }
        }

        private async Task<List<BuilderParticipant>> AddParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> ipoParticipants)
        {
            var functionalRoleParticipants =
                ipoParticipants.Where(p => p.InvitedFunctionalRole != null).ToList();
            var personsWithOids = ipoParticipants.Where(p => p.InvitedPerson?.AzureOid != null).ToList();
            var personsWithoutOids = ipoParticipants.Where(p => p.InvitedPerson != null && p.InvitedPerson.AzureOid == null)
                .ToList();
            var externalEmailParticipants = ipoParticipants.Where(p => p.InvitedExternalEmail != null).ToList();

            participants = functionalRoleParticipants.Count > 0
                ? await AddFunctionalRoleParticipantsAsync(invitation, participants, functionalRoleParticipants)
                : participants;
            participants = personsWithOids.Count > 0
                ? await AddPersonParticipantsWithOidsAsync(invitation, participants, personsWithOids)
                : participants;
            participants = AddExternalParticipant(invitation, participants, externalEmailParticipants);
            participants = AddPersonParticipantsWithoutOids(invitation, participants, personsWithoutOids);

            return participants;
        }

        private async Task<List<BuilderParticipant>> AddFunctionalRoleParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> functionalRoleParticipants)
        {
            var codes = functionalRoleParticipants.Select(p => p.InvitedFunctionalRole.Code).ToList();
            var functionalRoles =
                await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, codes);

            foreach (var participant in functionalRoleParticipants)
            {
                var fr = functionalRoles.SingleOrDefault(p => p.Code == participant.InvitedFunctionalRole.Code);
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
                    foreach (var person in participant.InvitedFunctionalRole.InvitedPersons)
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
                            participants = InvitationHelper.AddPersonToOutlookParticipantList(frPerson, participants, person.Required);
                        }
                    }
                }
                else
                {
                    throw new IpoValidationException(
                        $"Could not find functional role with functional role code '{participant.InvitedFunctionalRole.Code}' on participant {participant.Organization}.");
                }
            }
            return participants;
        }

        private async Task<List<BuilderParticipant>> AddPersonParticipantsWithOidsAsync(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> personParticipantsWithOids)
        {
            var personsAdded = new List<ParticipantsForCommand>();
            foreach (var participant in personParticipantsWithOids)
            {
                if (InvitationHelper.ParticipantIsSigningParticipant(participant))
                {
                    participants = await AddSigner(
                        invitation,
                        participants,
                        participant.InvitedPerson,
                        participant.SortKey,
                        participant.Organization);
                    personsAdded.Add(participant);
                }
            }

            personParticipantsWithOids.RemoveAll(p => personsAdded.Contains(p));

            var oids = personParticipantsWithOids.Where(p => p.SortKey > 1).Select(p => p.InvitedPerson.AzureOid.ToString()).ToList();
            var persons = oids.Count > 0
                ? await _personApiService.GetPersonsByOidsAsync(_plantProvider.Plant, oids)
                : new List<ProCoSysPerson>();
            if (persons.Any())
            {
                foreach (var participant in personParticipantsWithOids)
                {
                    var person = persons.SingleOrDefault(p => p.AzureOid == participant.InvitedPerson.AzureOid.ToString());
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
                        participants = InvitationHelper.AddPersonToOutlookParticipantList(person, participants);
                    }
                }
            }

            return participants;
        }

        private async Task<List<BuilderParticipant>> AddSigner(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IInvitedPersonForCommand invitedSigner,
            int sortKey,
            Organization organization)
        {
            var person = await _personApiService.GetPersonByOidWithPrivilegesAsync(
                _plantProvider.Plant,
                invitedSigner.AzureOid.ToString(),
                _objectName,
                _signerPrivileges);
            if (person != null)
            {
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    organization,
                    IpoParticipantType.Person,
                    null,
                    person.FirstName,
                    person.LastName,
                    person.UserName,
                    person.Email,
                    new Guid(person.AzureOid),
                    sortKey));
                participants = InvitationHelper.AddPersonToOutlookParticipantList(person, participants);
            }
            else
            {
                throw new IpoValidationException(
                    $"Person does not have required privileges to be the {organization} participant.");
            }

            return participants;
        }

        private List<BuilderParticipant> AddPersonParticipantsWithoutOids(
            Invitation invitation,
            List<BuilderParticipant> participants,
            List<ParticipantsForCommand> personsParticipantsWithEmail)
        {
            foreach (var participant in personsParticipantsWithEmail)
            {
                //This code will only hit for users that do not have and azure oid (which all users should have).
                //Therefore, insert null for names - no endpoint in main is created to retrieve info from users based on email
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    participant.Organization,
                    IpoParticipantType.Person,
                    null,
                    null,
                    null,
                    null,
                    participant.InvitedPerson.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.InvitedPerson.Email)));
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
                    participant.InvitedExternalEmail.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.InvitedExternalEmail.Email)));
            }

            return participants;
        }

        private async Task<List<CommPkg>> GetCommPkgsToAddAsync(IList<string> commPkgScope, string projectName)
        {
            var commPkgDetailsList =
                await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, projectName, commPkgScope);

            if (commPkgDetailsList.Count != commPkgScope.Count)
            {
                throw new IpoValidationException("Could not find all comm pkgs in scope.");
            }

            var initialCommPkg = commPkgDetailsList.FirstOrDefault();
            if (initialCommPkg != null)
            {
                var initialSystem = initialCommPkg.SystemSubString;
                if (commPkgDetailsList.Any(commPkg => commPkg.SystemSubString != initialSystem))
                {
                    throw new IpoValidationException("Comm pkg scope must be within a system.");
                }
            }
            return commPkgDetailsList.Select(c => new CommPkg(
                _plantProvider.Plant,
                projectName,
                c.CommPkgNo,
                c.Description,
                c.CommStatus,
                c.System)).ToList();
        }

        private async Task<List<McPkg>> GetMcPkgsToAddAsync(IList<string> mcPkgScope, string projectName)
        {
            var mcPkgDetailsList =
                await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, projectName, mcPkgScope);

            if (mcPkgDetailsList.Count != mcPkgScope.Count)
            {
                throw new IpoValidationException("Could not find all mc pkgs in scope.");
            }

            var initialMcPkg = mcPkgDetailsList.FirstOrDefault();
            if (initialMcPkg != null)
            {
                var initialSystem = initialMcPkg.SystemSubString;
                if (mcPkgDetailsList.Any(mcPkg => mcPkg.SystemSubString != initialSystem))
                {
                    throw new IpoValidationException("Mc pkg scope must be within a system.");
                }
            }

            return mcPkgDetailsList.Select(mc => new McPkg(
                    _plantProvider.Plant,
                    projectName,
                    mc.CommPkgNo,
                    mc.McPkgNo,
                    mc.Description,
                    mc.System)).ToList();
        }

        private async Task<Guid> CreateOutlookMeeting(
            CreateInvitationCommand request,
            IReadOnlyCollection<BuilderParticipant> participants,
            Invitation invitation)
        {
            foreach (var participant in participants)
            {
                _logger.LogInformation($"Adding {participant.Person.AzureUniqueId} - {participant.Person.Mail} to invitation {invitation.Id}");
            }

            var organizer = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());

            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                var baseUrl = InvitationHelper.GetBaseUrl(_meetingOptions.CurrentValue.PcsBaseUrl, _plantProvider.Plant);

                meetingBuilder
                    .StandaloneMeeting(InvitationHelper.GenerateMeetingTitle(invitation), request.Location)
                    .StartsOn(request.StartTime, request.EndTime)
                    .WithTimeZone("UTC")
                    .WithParticipants(participants)
                    .WithClassification(MeetingClassification.Open)
                    .EnableOutlookIntegration()
                    .WithInviteBodyHtml(InvitationHelper.GenerateMeetingDescription(invitation, baseUrl, organizer));
            });
            return meeting.Id;
        }
    }
}
