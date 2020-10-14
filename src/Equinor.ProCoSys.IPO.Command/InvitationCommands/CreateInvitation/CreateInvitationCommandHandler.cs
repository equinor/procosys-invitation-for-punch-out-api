using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;
using ParticipantType = Fusion.Integration.Meeting.ParticipantType;

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
            var invitation = new Invitation(_plantProvider.Plant, request.ProjectName, request.Title, request.Type);
            _invitationRepository.Add(invitation);

            if (request.CommPkgScope.Count > 0)
            {
                foreach (var commPkg in request.CommPkgScope)
                {
                    invitation.AddCommPkg(new CommPkg(
                        _plantProvider.Plant, 
                        request.ProjectName, 
                        commPkg.CommPkgNo, 
                        commPkg.Description,
                        commPkg.Status));
                }
            }

            if (request.McPkgScope.Count > 0)
            {
                foreach (var mcPkg in request.McPkgScope)
                {
                    invitation.AddMcPkg(new McPkg(
                        _plantProvider.Plant, 
                        request.ProjectName, 
                        mcPkg.CommPkgNo, 
                        mcPkg.McPkgNo, 
                        mcPkg.Description));
                }
            }

            foreach (var participant in request.Participants)
            {
                if (participant.Organization == Organization.External)
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        participant.Organization,
                        Domain.AggregateModels.InvitationAggregate.ParticipantType.Person,
                        null,
                        null,
                        null,
                        participant.ExternalEmail,
                        Guid.Empty,
                        participant.SortKey));
                    participants.Add(new BuilderParticipant(ParticipantType.Required,
                        new ParticipantIdentifier(participant.ExternalEmail)));
                }

                if (participant.Person != null)
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        participant.Organization,
                        Domain.AggregateModels.InvitationAggregate.ParticipantType.Person,
                        null,
                        participant.Person.FirstName,
                        participant.Person.LastName,
                        participant.Person.Email,
                        participant.Person.AzureOid,
                        participant.SortKey));

                    if (participant.Person.AzureOid == Guid.Empty)
                    {
                        participants.Add(new BuilderParticipant(ParticipantType.Required,
                            new ParticipantIdentifier(participant.Person.Email)));
                    }
                    else
                    {
                        participants.Add(new BuilderParticipant(ParticipantType.Required,
                            new ParticipantIdentifier(participant.Person.AzureOid)));
                    }
                }

                if (participant.FunctionalRole != null)
                {
                    if (!participant.FunctionalRole.UsePersonalEmail)
                    {
                        invitation.AddParticipant(new Participant(
                            _plantProvider.Plant,
                            participant.Organization,
                            Domain.AggregateModels.InvitationAggregate.ParticipantType.FunctionalRole,
                            participant.FunctionalRole.Code,
                            null,
                            null,
                            participant.FunctionalRole.Email,
                            Guid.Empty,
                            participant.SortKey));
                        participants.Add(new BuilderParticipant(ParticipantType.Required,
                            new ParticipantIdentifier(participant.FunctionalRole.Email)));
                    }
                    if (participant.FunctionalRole.Persons != null) 
                    { 
                        foreach (var p in participant.FunctionalRole.Persons)
                        {
                            invitation.AddParticipant(new Participant(
                                _plantProvider.Plant,
                                participant.Organization,
                                Domain.AggregateModels.InvitationAggregate.ParticipantType.FunctionalRole,
                                participant.FunctionalRole.Code,
                                p.FirstName,
                                p.LastName,
                                p.Email,
                                p.AzureOid,
                                participant.SortKey));
                            if (p.AzureOid == Guid.Empty)
                            {
                                participants.Add(new BuilderParticipant(p.Required ? ParticipantType.Required : ParticipantType.Optional,
                                    new ParticipantIdentifier(p.Email)));
                            }
                            else
                            {
                                participants.Add(new BuilderParticipant(p.Required ? ParticipantType.Required : ParticipantType.Optional,
                                    new ParticipantIdentifier(p.AzureOid)));
                            }
                        }
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var meeting = await _meetingClient.CreateMeetingAsync(meetingBuilder =>
            {
                meetingBuilder
                .StandaloneMeeting(request.Title, request.Location)
                .StartsOn(request.StartTime, request.EndTime)
                .WithParticipants(participants)
                .EnableOutlookIntegration(OutlookMode.All)
                .WithClassification(MeetingClassification.Restricted)
                .WithInviteBodyHtml(request.BodyHtml);
            });
            invitation.MeetingId = meeting.Id;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult<int>(invitation.Id);
        }
    }
}
