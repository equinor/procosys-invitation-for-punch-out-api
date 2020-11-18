using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation
{
    public class SignInvitationCommandValidator : AbstractValidator<SignInvitationCommand>
    {
        public SignInvitationCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => BeAnExistingParticipant(command.ParticipantId, command.InvitationId, token))
                .WithMessage(command =>
                    $"Participant with this ID does not exist! Id={command.ParticipantId}")
                .MustAsync((command, token) => BeANonCanceledInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    "Invitation is canceled, and thus cannot be signed!")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, token) => BeASigningParticipantOnIpo(command.InvitationId, command.ParticipantId, token))
                .WithMessage(command =>
                    $"The IPO does not have a participant assigned to sign the IPO with this ID! ParticipantId={command.ParticipantId}")
                .MustAsync((command, token) => BeTheAssignedPersonIfPersonParticipant(command.InvitationId,command.ParticipantId, token))
                .WithMessage(command =>
                    "Person signing is not assigned to sign IPO, or there is not a valid functional role on the IPO!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> BeANonCanceledInvitation(int invitationId, CancellationToken token)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, token);

            async Task<bool> BeASigningParticipantOnIpo(int invitationId, int participantId, CancellationToken token)
                => await invitationValidator.SignerExistsAsync(invitationId, participantId, token);

            async Task<bool> BeTheAssignedPersonIfPersonParticipant(int invitationId, int participantId, CancellationToken token)
                => await invitationValidator.ValidSigningParticipantExistsAsync(invitationId, participantId, token);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken token)
                => await invitationValidator.ParticipantExistsAsync(participantId, invitationId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
