﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class GetInvitationByIdQueryHandler : IRequestHandler<GetInvitationByIdQuery, Result<InvitationDto>>
    {
        private readonly IReadOnlyContext _context;
        private readonly IFusionMeetingClient _meetingClient;

        public GetInvitationByIdQueryHandler(IReadOnlyContext context, IFusionMeetingClient meetingClient)
        {
            _context = context;
            _meetingClient = meetingClient;
        }

        public async Task<Result<InvitationDto>> Handle(GetInvitationByIdQuery request, CancellationToken token)
        {
            var invitation = await _context.QuerySet<Invitation>()
                .Include(i => i.CommPkgs)
                .Include(i => i.McPkgs)
                .Include(i => i.Participants)
                .SingleOrDefaultAsync(x => x.Id == request.InvitationId, token);

            if (invitation == null)
            {
                return new NotFoundResult<InvitationDto>(Strings.EntityNotFound(nameof(Invitation), request.InvitationId));
            }

            var createdBy = await _context.QuerySet<Person>().SingleOrDefaultAsync(p => p.Id == invitation.CreatedById, token);

            if (createdBy == null)
            {
                return new NotFoundResult<InvitationDto>($"Could not get person that created the IPO with id {invitation.CreatedById}");
            }

            var createdByName = createdBy.FirstName + ' ' + createdBy.LastName;

            GeneralMeeting meeting;
            try
            {
                meeting = await _meetingClient.GetMeetingAsync(invitation.MeetingId,
                    query => query.ExpandInviteBodyHtml().ExpandProperty("participants.outlookstatus"));
            }
            catch (Exception e)
            {
                meeting = null; //user has not been invited to IPO with their personal email
            }

            var invitationDto = ConvertToInvitationDto(invitation, createdByName, meeting);

            return new SuccessResult<InvitationDto>(invitationDto);
        }

        private static InvitationDto ConvertToInvitationDto(Invitation invitation, string createdBy, GeneralMeeting meeting)
        {
            var invitationResult = new InvitationDto(
                invitation.ProjectName,
                invitation.Title,
                invitation.Description,
                invitation.Location,
                invitation.Type,
                invitation.Status,
                createdBy,
                invitation.StartTimeUtc,
                invitation.EndTimeUtc,
                invitation.ObjectGuid,
                invitation.RowVersion.ConvertToString())
            {
                Participants = ConvertToParticipantDto(invitation.Participants),
                McPkgScope = ConvertToMcPkgDto(invitation.McPkgs),
                CommPkgScope = ConvertToCommPkgDto(invitation.CommPkgs)
            };

            AddParticipantTypeAndOutlookResponseToParticipants(meeting, invitationResult);

            return invitationResult;
        }

        private static void AddParticipantTypeAndOutlookResponseToParticipants(GeneralMeeting meeting, InvitationDto invitationResult)
        {
            foreach (var participant in invitationResult.Participants)
            {
                if (participant.Person != null)
                {
                    var participantPersonResponse = meeting != null
                        ? GetOutlookResponseByEmailAsync(meeting, participant.Person?.Person?.Email)
                        : null;
                    participant.Person.Response = participantPersonResponse;
                }

                if (participant.FunctionalRole?.Persons != null)
                {
                    foreach (var personInFunctionalRole in participant.FunctionalRole?.Persons)
                    {
                        var participantType = meeting != null
                            ? GetParticipantTypeByEmail(meeting, personInFunctionalRole.Person.Email)
                            : null;
                        personInFunctionalRole.Required = participantType.Equals(ParticipantType.Required);

                        var functionalRolePersonResponse = meeting != null
                            ? GetOutlookResponseByEmailAsync(meeting, personInFunctionalRole.Person.Email)
                            : null;
                        personInFunctionalRole.Response = functionalRolePersonResponse;
                    }
                }

                if (participant.ExternalEmail != null)
                {
                    var externalEmailResponse = meeting != null
                        ? GetOutlookResponseByEmailAsync(meeting, participant.ExternalEmail?.ExternalEmail)
                        : null;
                    participant.ExternalEmail.Response = externalEmailResponse;
                }

                if (participant.FunctionalRole?.Email != null)
                {
                    var functionalRoleResponse = meeting != null
                        ? GetOutlookResponseByEmailAsync(meeting, participant.FunctionalRole.Email)
                        : null;
                    participant.FunctionalRole.Response = functionalRoleResponse;
                }
            }
        }

        private static OutlookResponse? GetOutlookResponseByEmailAsync(GeneralMeeting meeting, string email)
            => meeting.Participants.FirstOrDefault(p 
            => string.Equals(p.Person.Mail, email, StringComparison.CurrentCultureIgnoreCase))
            ?.OutlookResponse;

        private static ParticipantType? GetParticipantTypeByEmail(GeneralMeeting meeting, string email) 
            => meeting.Participants.FirstOrDefault(p => p.Person.Mail == email)?.Type;

        private static IEnumerable<CommPkgScopeDto> ConvertToCommPkgDto(IEnumerable<CommPkg> commPkgs)
            => commPkgs.Select(commPkg => new CommPkgScopeDto(commPkg.CommPkgNo, commPkg.Description, commPkg.Status));

        private static IEnumerable<McPkgScopeDto> ConvertToMcPkgDto(IEnumerable<McPkg> mcPkgs) 
            => mcPkgs.Select(mcPkg => new McPkgScopeDto(mcPkg.McPkgNo, mcPkg.Description, mcPkg.CommPkgNo));

        private static IEnumerable<ParticipantDto> ConvertToParticipantDto(IReadOnlyCollection<Participant> participants)
        {
            var participantDtos = new List<ParticipantDto>();

            foreach (var participant in participants)
            {
                if (participant.Type == IpoParticipantType.FunctionalRole)
                {
                    var personsInFunctionalRole = participants
                        .Where(p => p.FunctionalRoleCode == participant.FunctionalRoleCode
                                    && p.SortKey == participant.SortKey
                                    && p.Type == IpoParticipantType.Person);

                    participantDtos.Add(new ParticipantDto(
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        null,
                        null,
                        ConvertToFunctionalRoleDto(participant, personsInFunctionalRole)));
                }
                else if (ParticipantIsNotInFunctionalRole(participant) && participant.Organization != Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        null,
                        ConvertToInvitedPersonDto(participant), 
                        null));
                }
                else if (participant.Organization == Organization.External)
                {
                    participantDtos.Add(new ParticipantDto(
                        participant.Organization,
                        participant.SortKey,
                        participant.SignedBy,
                        participant.SignedAtUtc,
                        participant.Note,
                        participant.Attended,
                        new ExternalEmailDto(participant.Id, participant.Email,
                            participant.RowVersion.ConvertToString()),
                        null,
                        null));
                }
            }

            return participantDtos;
        }

        private static bool ParticipantIsNotInFunctionalRole(Participant participant) => string.IsNullOrWhiteSpace(participant.FunctionalRoleCode);

        private static FunctionalRoleDto ConvertToFunctionalRoleDto(Participant participant, IEnumerable<Participant> personsInFunctionalRole)
            => new FunctionalRoleDto(participant.FunctionalRoleCode, participant.Email, ConvertToInvitedPersonDto(personsInFunctionalRole), participant.RowVersion.ConvertToString()) {Id = participant.Id};

        private static InvitedPersonDto ConvertToInvitedPersonDto(Participant participant)
            => new InvitedPersonDto(new PersonDto(participant.Id, participant.FirstName, participant.LastName, participant.AzureOid ?? Guid.Empty, participant.Email, participant.RowVersion.ConvertToString())); 

        private static IEnumerable<InvitedPersonDto> ConvertToInvitedPersonDto(IEnumerable<Participant> personsInFunctionalRole) 
            => personsInFunctionalRole.Select(ConvertToInvitedPersonDto).ToList();
    }
}
