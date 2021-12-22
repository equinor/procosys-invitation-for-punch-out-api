﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
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
        private const string _objectName = "IPO";
        private readonly IList<string> _signerPrivileges = new List<string> { "SIGN" };

        private readonly IInvitationRepository _invitationRepository;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IPersonApiService _personApiService;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;
        private readonly IPersonRepository _personRepository;

        public EditInvitationCommandHandler(
            IInvitationRepository invitationRepository, 
            IFusionMeetingClient meetingClient,
            IPlantProvider plantProvider,
            IUnitOfWork unitOfWork,
            IMcPkgApiService mcPkgApiService,
            ICommPkgApiService commPkgApiService,
            IPersonApiService personApiService,
            IFunctionalRoleApiService functionalRoleApiService,
            IOptionsMonitor<MeetingOptions> meetingOptions,
            IPersonRepository personRepository)
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
            _personRepository = personRepository;
        }

        public async Task<Result<string>> Handle(EditInvitationCommand request, CancellationToken cancellationToken)
        {
            var participants = new List<BuilderParticipant>();
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            
            var mcPkgScope = await GetMcPkgScopeAsync(request.UpdatedMcPkgScope, invitation.ProjectName);
            var commPkgScope = await GetCommPkgScopeAsync(request.UpdatedCommPkgScope, invitation.ProjectName);
            participants = await UpdateParticipants(participants, request.UpdatedParticipants, invitation);

            invitation.EditIpo(
                request.Title,
                request.Description,
                request.Type,
                request.StartTime,
                request.EndTime,
                request.Location,
                mcPkgScope,
                commPkgScope);

            invitation.SetRowVersion(request.RowVersion);
            try
            {
                var baseUrl =
                    InvitationHelper.GetBaseUrl(_meetingOptions.CurrentValue.PcsBaseUrl, _plantProvider.Plant);

                var organizer = await _personRepository.GetByIdAsync(invitation.CreatedById);
                await _meetingClient.UpdateMeetingAsync(invitation.MeetingId, builder =>
                {
                    builder.UpdateLocation(request.Location);
                    builder.UpdateMeetingDate(request.StartTime, request.EndTime);
                    builder.UpdateTimeZone("UTC");
                    builder.UpdateParticipants(participants);
                    builder.UpdateInviteBodyHtml(InvitationHelper.GenerateMeetingDescription(invitation, baseUrl, organizer));
                });
            }
            catch (Exception e)
            {
                throw new Exception("Error: Could not update outlook meeting.", e);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private async Task<List<McPkg>> GetMcPkgScopeAsync(IList<string> mcPkgNos, string projectName)
        {
            if (mcPkgNos.Count > 0)
            {
                var mcPkgsFromMain =
                    await _mcPkgApiService.GetMcPkgsByMcPkgNosAsync(_plantProvider.Plant, projectName, mcPkgNos);

                if (mcPkgsFromMain.Count != mcPkgNos.Count)
                {
                    throw new IpoValidationException("Could not find all mc pkgs in scope.");
                }

                var initialMcPkg = mcPkgsFromMain.FirstOrDefault();
                if (initialMcPkg != null)
                {
                    var initialSystem = initialMcPkg.SystemSubString;
                    if (mcPkgsFromMain.Any(mcPkg => mcPkg.SystemSubString != initialSystem))
                    {
                        throw new IpoValidationException("Mc pkg scope must be within a system.");
                    }
                }

                return mcPkgsFromMain.Select(mc => new McPkg(
                    _plantProvider.Plant,
                    projectName,
                    mc.CommPkgNo,
                    mc.McPkgNo,
                    mc.Description,
                    mc.System)).ToList();
            }

            return new List<McPkg>();
        }

        private async Task<List<CommPkg>> GetCommPkgScopeAsync(IList<string> commPkgNos, string projectName)
        {
            if (commPkgNos.Count > 0)
            {
                var commPkgsFromMain =
                    await _commPkgApiService.GetCommPkgsByCommPkgNosAsync(_plantProvider.Plant, projectName, commPkgNos);

                if (commPkgsFromMain.Count != commPkgNos.Count)
                {
                    throw new IpoValidationException("Could not find all comm pkgs in scope.");
                }

                var initialCommPkg = commPkgsFromMain.FirstOrDefault();
                if (initialCommPkg != null)
                {
                    var initialSystem = initialCommPkg.SystemSubString;

                    if (commPkgsFromMain.Any(commPkg => commPkg.SystemSubString != initialSystem))
                    {
                        throw new IpoValidationException("Comm pkg scope must be within a system.");
                    }
                }

                return commPkgsFromMain.Select(c => new CommPkg(
                    _plantProvider.Plant,
                    projectName,
                    c.CommPkgNo,
                    c.Description,
                    c.CommStatus,
                    c.System)).ToList();
            }

            return new List<CommPkg>();
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
            var externalEmailParticipantsIds = externalEmailParticipants.Select(p => p.ExternalEmail.Id).ToList();

            var participantsToUpdateIds = externalEmailParticipantsIds
                .Concat(personsWithOidsIds)
                .Concat(personParticipantsWithEmailsIds)
                .Concat(functionalRoleParticipantIds).ToList();
            participantsToUpdateIds.AddRange(from fr in functionalRoleParticipants where fr.Person != null select fr.Person.Id);
            foreach (var functionalRoleParticipant in functionalRoleParticipants)
            {
                participantsToUpdateIds.AddRange(functionalRoleParticipant.FunctionalRole.Persons.Select(person => person.Id));
            }

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
            participants = AddPersonParticipantsWithoutOids(invitation, participants, personParticipantsWithEmails, existingParticipants);

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

                            participants = InvitationHelper.AddPersonToOutlookParticipantList(frPerson, participants, person.Required);
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
            List<ParticipantsForCommand> personParticipantsWithOids,
            IList<Participant> existingParticipants)
        {
            var personsAdded = new List<ParticipantsForCommand>();

            foreach (var participant in personParticipantsWithOids)
            {
                if (InvitationHelper.ParticipantIsSigningParticipant(participant))
                {
                    participants = await AddSigner(
                        invitation,
                        participants,
                        existingParticipants,
                        participant.Person,
                        participant.SortKey,
                        participant.Organization);
                    personsAdded.Add(participant);
                }
            }

            personParticipantsWithOids.RemoveAll(p => personsAdded.Contains(p));

            var oids = personParticipantsWithOids.Where(p => p.SortKey > 1)
                .Select(p => p.Person.AzureOid.ToString())
                .ToList();
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

                        participants = InvitationHelper.AddPersonToOutlookParticipantList(person, participants);
                    }
                }
            }

            return participants;
        }

        private async Task<List<BuilderParticipant>> AddSigner(
            Invitation invitation,
            List<BuilderParticipant> participants,
            IList<Participant> existingParticipants,
            PersonForCommand person,
            int sortKey,
            Organization organization)
        {
            var personFromMain = await _personApiService.GetPersonByOidWithPrivilegesAsync(_plantProvider.Plant,
                person.AzureOid.ToString(), _objectName, _signerPrivileges);
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
                participants = InvitationHelper.AddPersonToOutlookParticipantList(personFromMain, participants);
            }
            else
            {
                throw new IpoValidationException($"Person does not have required privileges to be the {organization} participant.");
            }
            return participants;
        }

        private List<BuilderParticipant> AddPersonParticipantsWithoutOids(
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
                        null,
                        null,
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
                        null,
                        null,
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
    }
}
