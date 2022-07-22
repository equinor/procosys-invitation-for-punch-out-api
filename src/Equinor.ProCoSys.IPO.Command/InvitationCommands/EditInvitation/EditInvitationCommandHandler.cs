using System;
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
            var meetingParticipants = new List<BuilderParticipant>();
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            
            var mcPkgScope = await GetMcPkgScopeAsync(request.UpdatedMcPkgScope, invitation.ProjectName);
            var commPkgScope = await GetCommPkgScopeAsync(request.UpdatedCommPkgScope, invitation.ProjectName);
            meetingParticipants = await UpdateParticipants(meetingParticipants, request.UpdatedParticipants, invitation);

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
                    builder.UpdateParticipants(meetingParticipants);
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

                var initialCommPkg = mcPkgsFromMain.FirstOrDefault();
                if (initialCommPkg != null)
                {
                    var initialSection = initialCommPkg.Section;
                    if (mcPkgsFromMain.Any(commPkg => commPkg.Section != initialSection))
                    {
                        throw new IpoValidationException("Mc pkg scope must be within a section.");
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
                    var initialSection = initialCommPkg.Section;
                    if (commPkgsFromMain.Any(commPkg => commPkg.Section != initialSection))
                    {
                        throw new IpoValidationException("Comm pkg scope must be within a section.");
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
            List<BuilderParticipant> meetingParticipants,
            IList<ParticipantsForEditCommand> participantsToUpdate,
            Invitation invitation)
        {
            var existingParticipants = invitation.Participants.ToList();

            var functionalRoleParticipants =
                participantsToUpdate.Where(p => p.InvitedFunctionalRoleToEdit != null).ToList();
            var functionalRoleParticipantIds = functionalRoleParticipants.Select(p => p.InvitedFunctionalRoleToEdit.Id).ToList();

            var personsWithOids = participantsToUpdate.Where(p => p.InvitedPersonToEdit?.AzureOid != null).ToList();
            var personsWithOidsIds = personsWithOids.Select(p => p.InvitedPersonToEdit.Id).ToList();

            var personParticipantsWithEmails = participantsToUpdate.Where(p => p.InvitedPersonToEdit != null && p.InvitedPersonToEdit.AzureOid == null)
                .ToList();
            var personParticipantsWithEmailsIds = personParticipantsWithEmails.Select(p => p.InvitedPersonToEdit.Id).ToList();

            var externalEmailParticipants = participantsToUpdate.Where(p => p.InvitedExternalEmailToEdit != null).ToList();
            var externalEmailParticipantsIds = externalEmailParticipants.Select(p => p.InvitedExternalEmailToEdit.Id).ToList();

            var participantsToUpdateIds = externalEmailParticipantsIds
                .Concat(personsWithOidsIds)
                .Concat(personParticipantsWithEmailsIds)
                .Concat(functionalRoleParticipantIds).ToList();
            participantsToUpdateIds.AddRange(from fr in functionalRoleParticipants where fr.InvitedPersonToEdit != null select fr.InvitedPersonToEdit.Id);
            foreach (var functionalRoleParticipant in functionalRoleParticipants)
            {
                participantsToUpdateIds.AddRange(functionalRoleParticipant.InvitedFunctionalRoleToEdit.EditPersons.Select(person => person.Id));
            }

            var participantsToDelete = existingParticipants.Where(p => !participantsToUpdateIds.Contains(p.Id));
            foreach (var participantToDelete in participantsToDelete)
            {
                invitation.RemoveParticipant(participantToDelete);
                _invitationRepository.RemoveParticipant(participantToDelete);
            }

            meetingParticipants = functionalRoleParticipants.Count > 0
                ? await UpdateFunctionalRoleParticipantsAsync(invitation, meetingParticipants, functionalRoleParticipants, existingParticipants)
                : meetingParticipants;
            meetingParticipants = personsWithOids.Count > 0
                ? await AddPersonParticipantsWithOidsAsync(invitation, meetingParticipants, personsWithOids, existingParticipants)
                : meetingParticipants;
            meetingParticipants = AddExternalParticipant(invitation, meetingParticipants, externalEmailParticipants, existingParticipants);
            meetingParticipants = AddPersonParticipantsWithoutOids(invitation, meetingParticipants, personParticipantsWithEmails, existingParticipants);

            return meetingParticipants;
        }

        private async Task<List<BuilderParticipant>> UpdateFunctionalRoleParticipantsAsync(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
            IList<ParticipantsForEditCommand> functionalRoleParticipants,
            IList<Participant> existingParticipants)
        {
            var codes = functionalRoleParticipants.Select(p => p.InvitedFunctionalRoleToEdit.Code).ToList();
            var functionalRoles =
                await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, codes);

            foreach (var participant in functionalRoleParticipants)
            {
                var fr = functionalRoles.SingleOrDefault(p => p.Code == participant.InvitedFunctionalRole.Code);
                if (fr != null)
                {
                    var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedFunctionalRoleToEdit.Id);
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
                            participant.InvitedFunctionalRoleToEdit.RowVersion);
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
                        meetingParticipants.AddRange(InvitationHelper.SplitAndCreateOutlookParticipantsFromEmailList(fr.Email));
                    }
                    if (fr.InformationEmail != null)
                    {
                        meetingParticipants.AddRange(InvitationHelper.SplitAndCreateOutlookParticipantsFromEmailList(fr.InformationEmail));
                    }
                    foreach (var person in participant.InvitedFunctionalRoleToEdit.EditPersons)
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
                                    participant.InvitedFunctionalRole.Code,
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
            List<ParticipantsForEditCommand> personParticipantsWithOids,
            IList<Participant> existingParticipants)
        {
            var personsAdded = new List<ParticipantsForCommand>();

            foreach (var participant in personParticipantsWithOids)
            {
                if (InvitationHelper.ParticipantIsSigningParticipant(participant))
                {
                    meetingParticipants = await AddSigner(
                        invitation,
                        meetingParticipants,
                        existingParticipants,
                        participant.InvitedPersonToEdit,
                        participant.SortKey,
                        participant.Organization);
                    personsAdded.Add(participant);
                }
            }

            personParticipantsWithOids.RemoveAll(p => personsAdded.Contains(p));

            var oids = personParticipantsWithOids.Where(p => p.SortKey > 1)
                .Select(p => p.InvitedPersonToEdit.AzureOid.ToString())
                .ToList();
            var persons = oids.Count > 0
                ? await _personApiService.GetPersonsByOidsAsync(_plantProvider.Plant, oids)
                : new List<ProCoSysPerson>();
            if (persons.Any())
            {
                foreach (var participant in personParticipantsWithOids)
                {
                    var person = persons.SingleOrDefault(p => p.AzureOid == participant.InvitedPersonToEdit.AzureOid.ToString());
                    if (person != null)
                    {
                        var existingParticipant =
                            existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedPersonToEdit.Id);
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
                                participant.InvitedPersonToEdit.RowVersion);
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

                        meetingParticipants = InvitationHelper.AddPersonToOutlookParticipantList(person, meetingParticipants);
                    }
                }
            }

            return meetingParticipants;
        }

        private async Task<List<BuilderParticipant>> AddSigner(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
            IList<Participant> existingParticipants,
            InvitedPersonForEditCommand person,
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
                meetingParticipants = InvitationHelper.AddPersonToOutlookParticipantList(personFromMain, meetingParticipants);
            }
            else
            {
                throw new IpoValidationException($"Person does not have required privileges to be the {organization} participant.");
            }
            return meetingParticipants;
        }

        private List<BuilderParticipant> AddPersonParticipantsWithoutOids(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
            IEnumerable<ParticipantsForEditCommand> personsParticipantsWithEmail,
            IList<Participant> existingParticipants)
        {
            foreach (var participant in personsParticipantsWithEmail)
            {
                var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedPersonToEdit.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        null,
                        null,
                        participant.InvitedPersonToEdit.Email,
                        null,
                        participant.SortKey,
                        participant.InvitedPersonToEdit.RowVersion);
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
                        participant.InvitedPersonToEdit.Email,
                        null,
                        participant.SortKey));
                }
                meetingParticipants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.InvitedExternalEmail.Email)));
            }
            return meetingParticipants;
        }

        private List<BuilderParticipant> AddExternalParticipant(
            Invitation invitation,
            List<BuilderParticipant> meetingParticipants,
            IEnumerable<ParticipantsForEditCommand> participantsWithExternalEmail,
            IList<Participant> existingParticipants)
        {
            foreach (var participant in participantsWithExternalEmail)
            {
                var existingParticipant =
                    existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedExternalEmailToEdit.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        null,
                        null,
                        participant.InvitedExternalEmailToEdit.Email,
                        null,
                        participant.SortKey,
                        participant.InvitedExternalEmailToEdit.RowVersion);
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
                        participant.InvitedExternalEmailToEdit.Email,
                        null,
                        participant.SortKey));
                }
                meetingParticipants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.InvitedExternalEmail.Email)));
            }
            return meetingParticipants;
        }
    }
}
