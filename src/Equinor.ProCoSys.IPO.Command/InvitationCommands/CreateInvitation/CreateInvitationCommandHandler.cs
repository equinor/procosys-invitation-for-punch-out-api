using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<int>>
    {
        private readonly IPlantProvider _plantProvider;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateInvitationCommandHandler(
            IPlantProvider plantProvider,
            IFusionMeetingClient meetingClient,
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork)
        {
            _plantProvider = plantProvider;
            _meetingClient = meetingClient;
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
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

            foreach (var participant in request.Participants)
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            try
            {
                var meetingId = await CreateOutlookMeeting(request, participants);
                invitation.MeetingId = meetingId;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new SuccessResult<int>(invitation.Id);
            }
            catch
            {
                _invitationRepository.Remove(invitation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new UnexpectedResult<int>("Error: Could not create outlook meeting.");
            }
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
                participant.ExternalEmail,
                null,
                participant.SortKey));
            participants.Add(new BuilderParticipant(ParticipantType.Required,
                new ParticipantIdentifier(participant.ExternalEmail)));
            return participants;
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
