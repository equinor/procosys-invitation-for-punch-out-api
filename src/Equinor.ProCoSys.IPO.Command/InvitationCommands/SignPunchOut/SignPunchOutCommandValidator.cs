﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignPunchOut
{
    public class SignPunchOutCommandValidator : AbstractValidator<SignPunchOutCommand>
    {
        public SignPunchOutCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeAnExistingParticipant(command.ParticipantId, command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Participant with ID does not exist on invitation! Id={command.ParticipantId}")
                .MustAsync((command, cancellationToken) => BeANonCanceledInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation is canceled, and thus cannot be signed!")
                .MustAsync((command, cancellationToken) => InvitationDoesntHaveStatusScopeHandedOver(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation has status ScopeHandedOver, and thus cannot be signed!")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, cancellationToken) => BeASigningParticipantOnIpo(command.InvitationId, command.ParticipantId, cancellationToken))
                .WithMessage(command =>
                    $"Participant is not assigned to sign this IPO! ParticipantId={command.ParticipantId}")
                .MustAsync((command, cancellationToken) => BeTheAssignedPersonIfPersonParticipant(command.InvitationId, command.ParticipantId, cancellationToken))
                .WithMessage(command =>
                    "Person signing is not assigned to sign IPO, or there is not a valid functional role on the IPO!")
                .MustAsync((command, cancellationToken) => BeUnsignedParticipant(command.ParticipantId, command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Participant is already signed!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeANonCanceledInvitation(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);

            async Task<bool> InvitationDoesntHaveStatusScopeHandedOver(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.ScopeHandedOver, cancellationToken);

            async Task<bool> BeASigningParticipantOnIpo(int invitationId, int participantId, CancellationToken cancellationToken)
                => await invitationValidator.SignerExistsAsync(invitationId, participantId, cancellationToken);

            async Task<bool> BeTheAssignedPersonIfPersonParticipant(int invitationId, int participantId, CancellationToken cancellationToken)
                => await invitationValidator.CurrentUserIsValidSigningParticipantAsync(invitationId, participantId, cancellationToken);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ParticipantExistsAsync(participantId, invitationId, cancellationToken);
            
            async Task<bool> BeUnsignedParticipant(int participantId, int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.ParticipantIsSignedAsync(participantId, invitationId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
