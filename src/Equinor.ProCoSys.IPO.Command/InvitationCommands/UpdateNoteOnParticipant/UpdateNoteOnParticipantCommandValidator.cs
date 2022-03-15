using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateNoteOnParticipant
{
    public class UpdateNoteOnParticipantCommandValidator : AbstractValidator<UpdateNoteOnParticipantCommand>
    {
        public UpdateNoteOnParticipantCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;
            // todo ha sjekk for status til invitasjonen
            RuleFor(command => command)
                .MustAsync((command, cancellationToken) =>
                    BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) =>
                    NotBeCancelledInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Cannot perform updates on cancelled invitation! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) =>
                    BeAnExistingParticipant(command.ParticipantId, command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Participant with ID does not exist on invitation! ParticipantId={command.ParticipantId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command =>
                    $"Participant doesn't have valid rowVersion! ParticipantRowVersion={command.RowVersion}")
                .MustAsync((command, cancellationToken) => HasPermissionToEditParticipant(command.ParticipantId, command.InvitationId, cancellationToken))
                .WithMessage("The current user does not have sufficient privileges to edit this participant.");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> NotBeCancelledInvitation(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ParticipantExistsAsync(participantId, invitationId, cancellationToken);

            async Task<bool> HasPermissionToEditParticipant(int participantId, int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.HasPermissionToEditParticipantAsync(participantId, invitationId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
