using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut
{
    public class CompletePunchOutCommandHandler : IRequestHandler<CompletePunchOutCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonRepository _personRepository;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;
        private readonly IEmailService _emailService;
        private readonly ILogger<CompletePunchOutCommandHandler> _logger;

        public CompletePunchOutCommandHandler(
            IInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IPersonRepository personRepository,
            IOptionsMonitor<MeetingOptions> meetingOptions,
            IEmailService emailService,
            ILogger<CompletePunchOutCommandHandler> logger)
        {
            _invitationRepository = invitationRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personRepository = personRepository;
            _meetingOptions = meetingOptions;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(CompletePunchOutCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);
            var currentUser = await _personRepository.GetByOidAsync(_currentUserProvider.GetCurrentUserOid());
            var completedAtUtc = DateTime.UtcNow;
            var participant = invitation.Participants.SingleOrDefault(p =>
                p.SortKey == 0 &&
                p.Organization == Organization.Contractor &&
                p.AzureOid == currentUser.Guid);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            if (participant == null || participant.FunctionalRoleCode != null)
            {
                var functionalRole = invitation.Participants
                    .SingleOrDefault(p => p.SortKey == 0 &&
                                          p.FunctionalRoleCode != null &&
                                          p.Type == IpoParticipantType.FunctionalRole);

                invitation.CompleteIpo(functionalRole, request.ParticipantRowVersion, currentUser, completedAtUtc);
            }
            else
            {
                invitation.CompleteIpo(participant, request.ParticipantRowVersion, currentUser, completedAtUtc);
            }
            UpdateAttendedStatusAndNotesOnParticipants(invitation, request.Participants);
            invitation.SetRowVersion(request.InvitationRowVersion);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await SendEmailAsync(invitation, cancellationToken);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"User with oid {_currentUserProvider.GetCurrentUserOid()} could not complete invitation {invitation.Id}. Error occured when sending email.");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new IpoSendMailException("It is currently not possible to complete invitation for punch-out since there is a problem when sending email. Please try again in a later. Contact support if the issue persists.", ex);
            }

            return new SuccessResult<string>(invitation.RowVersion.ConvertToString());
        }

        private void UpdateAttendedStatusAndNotesOnParticipants(Invitation invitation,
            IList<UpdateAttendedStatusAndNoteOnParticipantForCommand> participants)
        {
            foreach (var participant in participants)
            {
                var ipoParticipant = invitation.Participants.Single(p => p.Id == participant.Id);
                ipoParticipant.Note = participant.Note;
                ipoParticipant.Attended = participant.Attended;
                ipoParticipant.SetRowVersion(participant.RowVersion);
            }
        }

        private async Task SendEmailAsync(Invitation invitation, CancellationToken cancellationToken)
        {
            var emails = invitation.GetCompleterEmails();

            if (emails.Count == 0)
            {
                return;
            }

            var baseUrl = _meetingOptions.CurrentValue.PcsBaseUrl;
            var id = invitation.Id;
            var title = invitation.Title;
            var plantId = invitation.Plant.Split('$').Last();
            var subject = $"Completed notification: IPO-{id}";
            var body =
                $"<p>IPO-{id}: {title} has been completed and is ready for your attention to sign and accept.</p>" +
                "<p>Click the link to review " +
                $"<a href=\"{baseUrl}{plantId}/InvitationForPunchOut/{id}\">IPO-{id}</a>.</p>";

            await _emailService.SendEmailsAsync(emails, subject, body, cancellationToken);
        }
    }
}
