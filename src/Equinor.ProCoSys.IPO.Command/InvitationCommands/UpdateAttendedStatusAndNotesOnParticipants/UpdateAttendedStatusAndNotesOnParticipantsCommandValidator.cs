using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    public class UpdateAttendedStatusAndNotesOnParticipantsCommandValidator : AbstractValidator<UpdateAttendedStatusAndNotesOnParticipantsCommand>
    {
        public UpdateAttendedStatusAndNotesOnParticipantsCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeAnInvitationInCompletedStage(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation is not in completed stage, and thus cannot change attended statuses or notes!")
                .MustAsync((command, cancellationToken) => BeAContractorOnIpo(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "The IPO does not have a contractor assigned to the IPO!")
                .MustAsync((command, cancellationToken) => BeTheAssignedContractorIfPersonParticipant(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "User is not the contractor assigned to complete this IPO, or there is not a valid contractor on the IPO!");

            RuleForEach(command => command.Participants)
                .MustAsync((command, participant, _, cancellationToken) => BeAnExistingParticipant(participant.Id, command.InvitationId, cancellationToken))
                .WithMessage((command, participant) =>
                    $"Participant with ID does not exist on invitation! ParticipantId={participant.Id}")
                .Must((command, participant) => HaveAValidRowVersion(participant.RowVersion))
                .WithMessage((command, participant) =>
                    $"Participant doesn't have valid rowVersion! ParticipantRowVersion={participant.RowVersion}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAnInvitationInCompletedStage(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Completed, cancellationToken);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ParticipantExistsAsync(participantId, invitationId, cancellationToken);

            async Task<bool> BeTheAssignedContractorIfPersonParticipant(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ValidCompleterParticipantExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAContractorOnIpo(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoHasCompleterAsync(invitationId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
