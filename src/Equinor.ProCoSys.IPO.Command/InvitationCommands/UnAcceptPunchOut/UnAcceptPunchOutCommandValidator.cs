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
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeAnInvitationInAcceptedStage(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation is not in accepted stage, and thus cannot be unaccepted!")
                .Must(command => HaveAValidRowVersion(command.InvitationRowVersion))
                .WithMessage(command =>
                    $"Invitation row version is not valid! InvitationRowVersion={command.InvitationRowVersion}")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, cancellationToken) => BeAConstructionCompanyOnIpo(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "The IPO does not have a construction company assigned to accept the IPO!")
                .MustAsync((command, cancellationToken) => BeThePersonWhoAccepted(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Person trying to unaccept is not the person who accepted the IPO!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAnInvitationInAcceptedStage(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Accepted, cancellationToken);

            async Task<bool> BeAConstructionCompanyOnIpo(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ConstructionCompanyExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeThePersonWhoAccepted(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.SameUserUnAcceptingThatAcceptedAsync(invitationId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
