using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut
{
    public class UnAcceptPunchOutCommandValidator : AbstractValidator<UnAcceptPunchOutCommand>
    {
        public UnAcceptPunchOutCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => BeAnInvitationInAcceptedStage(command.InvitationId, token))
                .WithMessage(command =>
                    "Invitation is not in accepted stage, and thus cannot be unaccepted!")
                .Must(command => HaveAValidRowVersion(command.InvitationRowVersion))
                .WithMessage(command =>
                    $"Invitation row version is not valid! InvitationRowVersion={command.InvitationRowVersion}")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, token) => BeAConstructionCompanyOnIpo(command.InvitationId, token))
                .WithMessage(command =>
                    "The IPO does not have a construction company assigned to accept the IPO!")
                .MustAsync((command, token) => BeThePersonWhoAccepted(command.InvitationId, token))
                .WithMessage(command =>
                    "Person trying to unaccept is not the person who accepted the IPO!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> BeAnInvitationInAcceptedStage(int invitationId, CancellationToken token)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Accepted, token);

            async Task<bool> BeAConstructionCompanyOnIpo(int invitationId, CancellationToken token)
                => await invitationValidator.ConstructionCompanyExistsAsync(invitationId, token);

            async Task<bool> BeThePersonWhoAccepted(int invitationId, CancellationToken token)
                => await invitationValidator.SameUserUnAcceptingThatAcceptedAsync(invitationId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
