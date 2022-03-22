using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut
{
    public class CancelPunchOutCommandValidator : AbstractValidator<CancelPunchOutCommand>
    {
        public CancelPunchOutCommandValidator(
            IInvitationValidator invitationValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => InvitationIsNotCanceled(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"IPO is already canceled! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => InvitationIsNotAccepted(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"IPO is in accepted stage! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => CurrentUserIsCreatorOrIsInContractorFunctionalRoleOfInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Current user is not the creator of the invitation and not in Contractor Functional Role! Id={command.InvitationId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command =>
                    $"Invitation does not have valid rowVersion! RowVersion={command.RowVersion}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> CurrentUserIsCreatorOrIsInContractorFunctionalRoleOfInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.CurrentUserIsAdminOrCreatorOrCompletorAsync(invitationId, cancellationToken);

            async Task<bool> InvitationIsNotCanceled(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);

            async Task<bool> InvitationIsNotAccepted(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Accepted, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
