using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;
using ParticipantType = Fusion.Integration.Meeting.ParticipantType;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommandHandler : IRequestHandler<EditInvitationCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public EditInvitationCommandHandler(
            IInvitationRepository invitationRepository, 
            IFusionMeetingClient meetingClient,
            IPlantProvider plantProvider,
            IUnitOfWork unitOfWork)
        {
            _invitationRepository = invitationRepository;
            _meetingClient = meetingClient;
            _plantProvider = plantProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(EditInvitationCommand request, CancellationToken cancellationToken)
        {
            var participants = new List<BuilderParticipant>();
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            invitation.Title = request.Title;
            invitation.ProjectName = request.ProjectName;
            invitation.Type = request.Type;

            UpdateMcPkgs(request.UpdatedMcPkgScope, invitation);
            UpdateCommPkgs(request.UpdatedCommPkgScope, invitation);
            AddMcPkgs(invitation, request.NewMcPkgScope, request.ProjectName);
            AddCommPkgs(invitation, request.NewCommPkgScope, request.ProjectName);

            participants = UpdateParticipants(participants, request.UpdatedParticipants, invitation);

            foreach (var participant in request.NewParticipants)
            {
                if (participant.Organization == Organization.External)
                {
                    participants = AddExternalParticipant(invitation, participants, participant);
                }

                if (participant.Person != null)
                {
                    participants = AddPersonParticipant(
                        invitation,
                        participants,
                        participant.Person,
                        participant.Organization,
                        participant.SortKey);
                }

                if (participant.FunctionalRole != null)
                {
                    participants = AddFunctionalRoleParticipant(invitation, participants, participant);
                }
            }

            await _meetingClient.UpdateMeetingAsync(invitation.MeetingId, builder =>
            {
                builder.UpdateTitle(request.Title);
                builder.UpdateLocation(request.Location);
                builder.UpdateStartDate(request.StartTime);
                builder.UpdateEndDate(request.EndTime);
                builder.UpdateParticipants(participants);
                builder.InviteBodyHtml = request.Description;
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SuccessResult<Unit>(Unit.Value);
        }

        private void UpdateMcPkgs(IList<McPkgScopeForCommand> mcPkgs, Invitation invitation)
        {
            var updatedMcPkgIds = mcPkgs.Select(mc => mc.Id);
            var excludedMcPkgs = invitation.McPkgs.Where(mc => !updatedMcPkgIds.Contains(mc.Id)).ToList();
            foreach (var mcPkgToDelete in excludedMcPkgs)
            {
                invitation.RemoveMcPkg(mcPkgToDelete);
                _invitationRepository.RemoveMcPkg(mcPkgToDelete);
            }
        }

        private void AddMcPkgs(Invitation invitation, IEnumerable<McPkgScopeForCommand> mcPkgScope, string projectName)
        {
            foreach (var mcPkg in mcPkgScope)
            {
                invitation.AddMcPkg(new McPkg(
                    _plantProvider.Plant,
                    projectName,
                    mcPkg.CommPkgNo,
                    mcPkg.McPkgNo,
                    mcPkg.Description));
            }
        }

        private void UpdateCommPkgs(IList<CommPkgScopeForCommand> commPkgs, Invitation invitation)
        {
            var updatedCommPkgIds = commPkgs.Select(c => c.Id);
            var excludedMcPkgs = invitation.CommPkgs.Where(c => !updatedCommPkgIds.Contains(c.Id)).ToList();
            foreach (var commPkgToDelete in excludedMcPkgs)
            {
                invitation.RemoveCommPkg(commPkgToDelete);
                _invitationRepository.RemoveCommPkg(commPkgToDelete);
            }
        }

        private void AddCommPkgs(Invitation invitation, IEnumerable<CommPkgScopeForCommand> commPkgScope, string projectName)
        {
            foreach (var commPkg in commPkgScope)
            {
                invitation.AddCommPkg(new CommPkg(
                    _plantProvider.Plant,
                    projectName,
                    commPkg.CommPkgNo,
                    commPkg.Description,
                    commPkg.Status));
            }
        }

        private List<BuilderParticipant> UpdateParticipants(
            List<BuilderParticipant> participants,
            IEnumerable<ParticipantsForCommand> participantsToUpdate, 
            Invitation invitation)
        {
            var updatedParticipantIds = new List<int?>();
            foreach (var participant in participantsToUpdate)
            {
                if (participant.ExternalEmail != null)
                {
                    updatedParticipantIds.Add(participant.ExternalEmail.Id);
                    var participantToUpdate = invitation.Participants.Single(p => p.Id == participant.ExternalEmail.Id);
                    UpdateExternalParticipant(
                        participantToUpdate, 
                        participant.ExternalEmail.Email, 
                        participant.SortKey);
                    participants.Add(new BuilderParticipant(ParticipantType.Required,
                        new ParticipantIdentifier(participant.ExternalEmail.Email)));
                }
                else if (participant.Person != null)
                {
                    updatedParticipantIds.Add(participant.Person.Id);
                    var participantToUpdate = invitation.Participants.Single(p => p.Id == participant.Person.Id);
                    UpdatePersonParticipant(
                        participantToUpdate, 
                        participant.SortKey, 
                        participant.Organization, 
                        participant.Person);
                    AddPersonToParticipantList(participants, participant.Person);
                }
                else
                {
                    if (!participant.FunctionalRole.UsePersonalEmail)
                    {
                        updatedParticipantIds.Add(participant.FunctionalRole.Id);
                        var participantToUpdate = invitation.Participants.Single(p => p.Id == participant.FunctionalRole.Id);
                        UpdateFunctionalRoleParticipant(
                            participantToUpdate,
                            participant.SortKey,
                            participant.Organization,
                            participant.FunctionalRole);
                        participants.Add(new BuilderParticipant(ParticipantType.Required,
                            new ParticipantIdentifier(participant.FunctionalRole.Email)));
                    }

                    foreach (var person in participant.FunctionalRole.Persons)
                    {
                        updatedParticipantIds.Add(person.Id);
                        var participantToUpdate = invitation.Participants.Single(p => p.Id == person.Id);
                        UpdatePersonParticipant(
                            participantToUpdate,
                            participant.SortKey,
                            participant.Organization,
                            person,
                            participant.FunctionalRole.Code);
                        AddPersonToParticipantList(participants, person);
                    }
                }
            }
            var excludedParticipants = invitation.Participants.Where(p => !updatedParticipantIds.Contains(p.Id)).ToList();
            foreach (var participantToDelete in excludedParticipants)
            {
                invitation.RemoveParticipant(participantToDelete);
                _invitationRepository.RemoveParticipant(participantToDelete);
            }

            return participants;
        }

        private List<BuilderParticipant> AddPersonToParticipantList(
            List<BuilderParticipant> participants,
            PersonForCommand person
            )
        {
            if (person.AzureOid != null && person.AzureOid != Guid.Empty)
            {
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(person.AzureOid ?? Guid.Empty)));
            }
            else
            {
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(person.Email)));
            }

            return participants;
        }

        private void UpdateFunctionalRoleParticipant(
            Participant participant,
            int sortKey,
            Organization organization,
            FunctionalRoleForCommand fr)
        {
            participant.SortKey = sortKey;
            participant.Email = fr.Email;
            participant.FirstName = null;
            participant.LastName = null;
            participant.Organization = organization;
            participant.Type = IpoParticipantType.FunctionalRole;
            participant.AzureOid = null;
            participant.FunctionalRoleCode = fr.Code;
        }

        private void UpdatePersonParticipant(
            Participant participant, 
            int sortKey, 
            Organization organization, 
            PersonForCommand person, 
            string code = null)
        {
            participant.SortKey = sortKey;
            participant.Email = person.Email;
            participant.FirstName = person.FirstName;
            participant.LastName = person.LastName;
            participant.Organization = organization;
            participant.Type = IpoParticipantType.Person;
            participant.AzureOid = person.AzureOid;
            participant.FunctionalRoleCode = code;
        }

        private void UpdateExternalParticipant(
            Participant participant, 
            string externalEmail, 
            int sortKey)
        {
            participant.SortKey = sortKey;
            participant.Email = externalEmail;
            participant.FirstName = null;
            participant.LastName = null;
            participant.Organization = Organization.External;
            participant.Type = IpoParticipantType.Person;
            participant.AzureOid = null;
            participant.FunctionalRoleCode = null;
        }

        private List<BuilderParticipant> AddFunctionalRoleParticipant(
            Invitation invitation,
            List<BuilderParticipant> participants,
            ParticipantsForCommand participant)
        {
            if (!participant.FunctionalRole.UsePersonalEmail)
            {
                invitation.AddParticipant(new Participant(
                    _plantProvider.Plant,
                    participant.Organization,
                    IpoParticipantType.FunctionalRole,
                    participant.FunctionalRole.Code,
                    null,
                    null,
                    participant.FunctionalRole.Email,
                    null,
                    participant.SortKey));
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(participant.FunctionalRole.Email)));
            }

            foreach (var p in participant.FunctionalRole.Persons)
            {
                participants = AddPersonParticipant(
                    invitation,
                    participants,
                    p,
                    participant.Organization,
                    participant.SortKey);
            }
            return participants;
        }

        private List<BuilderParticipant> AddPersonParticipant(
            Invitation invitation,
            List<BuilderParticipant> participants,
            PersonForCommand person,
            Organization organization,
            int sortKey,
            string functionalRoleCode = null)
        {
            invitation.AddParticipant(new Participant(
                _plantProvider.Plant,
                organization,
                functionalRoleCode != null ? IpoParticipantType.FunctionalRole : IpoParticipantType.Person,
                functionalRoleCode,
                person.FirstName,
                person.LastName,
                person.Email,
                person.AzureOid,
                sortKey));

            return AddPersonToParticipantList(participants, person);
        }

        private List<BuilderParticipant> AddExternalParticipant(
            Invitation invitation,
            List<BuilderParticipant> participants,
            ParticipantsForCommand participant)
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
            return participants;
        }
    }
}
