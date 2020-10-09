﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommandHandler : IRequestHandler<EditInvitationCommand, Result<Unit>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IFusionMeetingClient _meetingClient;

        public EditInvitationCommandHandler(IInvitationRepository invitationRepository, IFusionMeetingClient meetingClient)
        {
            _invitationRepository = invitationRepository;
            _meetingClient = meetingClient;
        }

        public async Task<Result<Unit>> Handle(EditInvitationCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);

            var participants = new List<BuilderParticipant>();
            // By OID
            participants.AddRange(
                request.Meeting.ParticipantOids.Select(p =>
                    new BuilderParticipant(ParticipantType.Required, new ParticipantIdentifier(p))));
            // By Email
            participants.AddRange(
                request.Meeting.ParticipantEmails.Select(p =>
                    new BuilderParticipant(ParticipantType.Required, new ParticipantIdentifier(p))));

            var meeting = await _meetingClient.UpdateMeetingAsync(invitation.MeetingId, builder =>
            {
                builder.UpdateTitle(request.Meeting.Title);
                builder.UpdateLocation(request.Meeting.Location);
                builder.UpdateStartDate(request.Meeting.StartTime);
                builder.UpdateEndDate(request.Meeting.EndTime);
                builder.UpdateParticipants(participants);
                builder.InviteBodyHtml = request.Meeting.BodyHtml;
            });
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
