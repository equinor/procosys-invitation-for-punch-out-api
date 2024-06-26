﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.Extensions.Options;
using ServiceResult;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.ICalendar;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.MessageContracts;

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
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectApiService _projectApiService;
        private readonly ICalendarService _calendarService;
        private readonly IEmailService _emailService;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly ICreateEventHelper _eventHelper;

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
            IProjectRepository projectRepository,
            IProjectApiService projectApiService,
            ICalendarService calendarService,
            IEmailService emailService,
            IIntegrationEventPublisher integrationEventPublisher,
            ICreateEventHelper eventHelper,
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
            _projectRepository = projectRepository;
            _projectApiService = projectApiService;
            _calendarService = calendarService;
            _emailService = emailService;
            _integrationEventPublisher = integrationEventPublisher;
            _eventHelper = eventHelper;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var meetingParticipants = new List<BuilderParticipant>();
            var mcPkgs = new List<McPkg>();
            var commPkgs = new List<CommPkg>();

            var project = await GetOrCreateProjectAsync(request, cancellationToken);

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
                project,
                request.Title,
                request.Description,
                request.Type,
                request.StartTime,
                request.EndTime,
                request.Location,
                mcPkgs,
                commPkgs);
            _invitationRepository.Add(invitation);

            meetingParticipants = await AddParticipantsAsync(invitation, meetingParticipants, request.Participants.ToList());
        
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            //Manual publish instead of domain event handler in order to obtain the id of the invitation
            await PublishEventToBusAsync(invitation, cancellationToken);

            try
            {
                invitation.MeetingId = await CreateOutlookMeeting(request, meetingParticipants, invitation, project.Name);
            }
            catch (IpoSendMailException)
            {
                _logger.LogWarning("Trying to use fallback solution for creating outlook meeting since meeting API failed for user with oid {UserOid} and invitation id {InvitationId}.", _currentUserProvider.GetCurrentUserOid(), invitation.Id);
                var organizer = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());

                try
                {
                    var message = _calendarService.CreateMessage(invitation, project.Name, organizer, _meetingOptions?.CurrentValue?.PcsBaseUrl, request);
                    await _emailService.SendMessageAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogError(ex, "User with oid {UserOid} could not create outlook meeting for invitation {InvitationId} using backup solution of sending ics attachment through SMTP.", _currentUserProvider.GetCurrentUserOid(), invitation.Id);
                    throw new IpoSendMailException("It is currently not possible to create invitation for punch-out since there is a problem when sending email to recipients. Please try again in a minute. Contact support if the issue persists.",ex);
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            try
            { 
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _unitOfWork.Commit();
                return new SuccessResult<int>(invitation.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private async Task PublishEventToBusAsync(Invitation invitation, CancellationToken cancellationToken)
        {
            var invitationEvent = await _eventHelper.CreateInvitationEvent(invitation);
            await _integrationEventPublisher.PublishAsync(invitationEvent, cancellationToken);
        }

        private async Task<Project> GetOrCreateProjectAsync(CreateInvitationCommand request, CancellationToken cancellationToken) 
            => await _projectRepository.GetProjectOnlyByNameAsync(request.ProjectName) ?? await AddProjectAsync(request, cancellationToken);

        private async Task<Project> AddProjectAsync(CreateInvitationCommand request, CancellationToken cancellationToken)
        {
            var proCoSysProject = await _projectApiService.TryGetProjectAsync(_plantProvider.Plant, request.ProjectName);
            if (proCoSysProject is null)
            {
                throw new IpoValidationException(
                    $"Could not find ProCoSys project called {request.ProjectName} in plant {_plantProvider.Plant}");
            }

            var project = new Project(_plantProvider.Plant, request.ProjectName, proCoSysProject.Description, proCoSysProject.ProCoSysGuid);
            project.IsClosed = proCoSysProject.IsClosed;

            _projectRepository.Add(project);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return project;
        }

        private async Task<List<BuilderParticipant>> AddParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
            List<ParticipantsForCommand> ipoParticipants)
        {
            var functionalRoleParticipants =
                ipoParticipants.Where(p => p.InvitedFunctionalRole != null).ToList();
            var persons = ipoParticipants.Where(p => p.InvitedPerson != null).ToList();
            var externalEmailParticipants = ipoParticipants.Where(p => p.InvitedExternalEmail != null).ToList();

            meetingParticipants = functionalRoleParticipants.Count > 0
                ? await AddFunctionalRoleParticipantsAsync(invitation, meetingParticipants, functionalRoleParticipants)
                : meetingParticipants;
            meetingParticipants = persons.Count > 0
                ? await AddPersonParticipantsWithOidsAsync(invitation, meetingParticipants, persons)
                : meetingParticipants;
            meetingParticipants = AddExternalParticipant(invitation, meetingParticipants, externalEmailParticipants);

            return meetingParticipants;
        }

        private async Task<List<BuilderParticipant>> AddFunctionalRoleParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
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
                        meetingParticipants.AddRange(InvitationHelper.SplitAndCreateOutlookParticipantsFromEmailList(fr.Email));
                    }
                    if (fr.InformationEmail != null)
                    {
                        meetingParticipants.AddRange(InvitationHelper.SplitAndCreateOutlookParticipantsFromEmailList(fr.InformationEmail));
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
                            meetingParticipants = InvitationHelper.AddPersonToOutlookParticipantList(frPerson, meetingParticipants, person.Required);
                        }
                    }
                }
                else
                {
                    throw new IpoValidationException(
                        $"Could not find functional role with functional role code '{participant.InvitedFunctionalRole.Code}' on participant {participant.Organization}.");
                }
            }
            return meetingParticipants;
        }

        private async Task<List<BuilderParticipant>> AddPersonParticipantsWithOidsAsync(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
            List<ParticipantsForCommand> personParticipantsWithOids)
        {
            var personsAdded = new List<ParticipantsForCommand>();
            foreach (var participant in personParticipantsWithOids)
            {
                if (InvitationHelper.ParticipantIsSigningParticipant(participant))
                {
                    meetingParticipants = await AddSigner(
                        invitation,
                        meetingParticipants,
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
                        meetingParticipants = InvitationHelper.AddPersonToOutlookParticipantList(person, meetingParticipants);
                    }
                }
            }

            return meetingParticipants;
        }

        private async Task<List<BuilderParticipant>> AddSigner(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
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
                meetingParticipants = InvitationHelper.AddPersonToOutlookParticipantList(person, meetingParticipants);
            }
            else
            {
                throw new IpoValidationException(
                    $"Person does not have required privileges to be the {organization} participant.");
            }

            return meetingParticipants;
        }

        private List<BuilderParticipant> AddExternalParticipant(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
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
                meetingParticipants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.InvitedExternalEmail.Email)));
            }

            return meetingParticipants;
        }

        private async Task<List<CommPkg>> GetCommPkgsToAddAsync(IList<string> commPkgScope, string projectName)
        {
            var commPkgDetailsList =
                await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, projectName, commPkgScope);

            if (commPkgDetailsList.Count != commPkgScope.Count)
            {
                throw new IpoValidationException("Could not find all comm pkgs in scope.");
            }

            if (commPkgDetailsList.Any(c => c.OperationHandoverStatus == "ACCEPTED"))
            {
                throw new IpoValidationException("Comm pkgs with signed RFOC cannot be in scope. Comm pkgs with signed RFOC: " 
                    + string.Join(",", commPkgDetailsList
                        .Where(c => c.OperationHandoverStatus == "ACCEPTED")
                        .Select(c => c.CommPkgNo)
                        .ToList()));
            }

            var initialCommPkg = commPkgDetailsList.FirstOrDefault();
            if (initialCommPkg != null)
            {
                var initialSection = initialCommPkg.Section;
                if (commPkgDetailsList.Any(commPkg => commPkg.Section != initialSection))
                {
                    throw new IpoValidationException("Comm pkg scope must be within a section.");
                }
            }

            var project = await _projectRepository.GetProjectOnlyByNameAsync(projectName);

            return commPkgDetailsList.Select(c => new CommPkg(
                _plantProvider.Plant,
                project,
                c.CommPkgNo,
                c.Description,
                c.CommStatus,
                c.System,
                c.ProCoSysGuid)).ToList();
        }

        private async Task<List<McPkg>> GetMcPkgsToAddAsync(IList<string> mcPkgScope, string projectName)
        {
            var mcPkgDetailsList =
                await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, projectName, mcPkgScope);

            if (mcPkgDetailsList.Count != mcPkgScope.Count)
            {
                throw new IpoValidationException("Could not find all mc pkgs in scope.");
            }

            if (mcPkgDetailsList.Any(mc => mc.OperationHandoverStatus == "ACCEPTED"))
            {
                throw new IpoValidationException("Mc pkgs with signed RFOC cannot be in scope. Mc pkgs with signed RFOC: "
                    + string.Join(",", mcPkgDetailsList
                    .Where(mc => mc.OperationHandoverStatus == "ACCEPTED")
                    .Select(mc => mc.McPkgNo)
                    .ToList()));
            }

            var initialMcPkg = mcPkgDetailsList.FirstOrDefault();
            if (initialMcPkg != null)
            {
                var initialSection = initialMcPkg.Section;
                if (mcPkgDetailsList.Any(commPkg => commPkg.Section != initialSection))
                {
                    throw new IpoValidationException("Mc pkg scope must be within a section.");
                }
            }

            var project = await _projectRepository.GetProjectOnlyByNameAsync(projectName);

            return mcPkgDetailsList.Select(mc => new McPkg(
                    _plantProvider.Plant,
                    project,
                    mc.CommPkgNo,
                    mc.McPkgNo,
                    mc.Description,
                    mc.System,
                    mc.ProCoSysGuid,
                    mc.CommPkgGuid)).ToList();
        }

        private async Task<Guid> CreateOutlookMeeting(
            CreateInvitationCommand request,
            IReadOnlyCollection<BuilderParticipant> meetingParticipants,
            Invitation invitation,
            string projectName)
        {
            foreach (var meetingParticipant in meetingParticipants)
            {
                _logger.LogInformation($"Adding {meetingParticipant.Person.AzureUniqueId} - {meetingParticipant.Person.Mail} to invitation {invitation.Id}");
            }

            var organizer = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());

            GeneralMeeting meeting;

            try
            {
                meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
                {
                    var baseUrl = InvitationHelper.GetBaseUrl(_meetingOptions.CurrentValue.PcsBaseUrl, _plantProvider.Plant);

                    meetingBuilder
                        .StandaloneMeeting(InvitationHelper.GenerateMeetingTitle(
                            invitation,
                            projectName,
                            request.Type,
                            request.Type == DisciplineType.DP ? request.McPkgScope : request.CommPkgScope),
                            request.Location)
                        .StartsOn(request.StartTime, request.EndTime)
                    .WithTimeZone("UTC")
                        .WithParticipants(meetingParticipants)
                        .WithClassification(MeetingClassification.Open)
                        .EnableOutlookIntegration()
                        .WithInviteBodyHtml(InvitationHelper.GenerateMeetingDescription(invitation, baseUrl, organizer, projectName, false));

                    if (request.IsOnline)
                    {
                        meetingBuilder.EnableTeamsMeeting();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"User with oid {_currentUserProvider.GetCurrentUserOid()} could not create outlook meeting for invitation {invitation.Id} when using meeting API.");
                throw new IpoSendMailException();
            }
            return meeting.Id;
        }
    }


}
