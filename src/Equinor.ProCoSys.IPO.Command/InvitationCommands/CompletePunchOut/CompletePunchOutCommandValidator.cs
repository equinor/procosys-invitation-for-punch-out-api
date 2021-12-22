using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut
{
    public class CompletePunchOutCommandValidator : AbstractValidator<CompletePunchOutCommand>
    {
        public CompletePunchOutCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeAnInvitationInPlannedStage(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation is not in planned stage, and thus cannot be completed!")
                .Must(command => HaveAValidRowVersion(command.InvitationRowVersion))
                .WithMessage(command =>
                    $"Invitation row version is not valid! InvitationRowVersion={command.InvitationRowVersion}")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, cancellationToken) => BeACompleterOnIpo(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "The IPO does not have a contractor assigned to complete the IPO!")
                .MustAsync((command, cancellationToken) => BeTheAssignedPersonIfPersonParticipant(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Person signing is not the contractor assigned to complete this IPO, or there is not a valid contractor on the IPO!");

            RuleForEach(command => command.Participants)
                .MustAsync((command, participant, _, cancellationToken) => BeAnExistingParticipant(participant.Id, command.InvitationId, cancellationToken))
                .WithMessage((command, participant) =>
                    $"Participant with ID does not exist on invitation! Participant={participant.Id}")
                .Must((command, participant) => HaveAValidRowVersion(participant.RowVersion))
                .WithMessage((command, participant) =>
                    $"Participant doesn't have valid rowVersion! Participant={participant.Id}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAnInvitationInPlannedStage(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Planned, cancellationToken);

            async Task<bool> BeACompleterOnIpo(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoHasCompleterAsync(invitationId, cancellationToken);

            async Task<bool> BeTheAssignedPersonIfPersonParticipant(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ValidCompleterParticipantExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ParticipantExistsAsync(participantId, invitationId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
