using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeletePunchOut
{
    public class DeletePunchOutCommandValidator : AbstractValidator<DeletePunchOutCommand>
    {
        public DeletePunchOutCommandValidator(
            IInvitationValidator invitationValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => InvitationIsCanceled(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"IPO is not canceled! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => CurrentUserIsCreatorOrOfInvitationOrAdmin(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"Current user is not the creator of the invitation and not in Contractor Functional Role! Id={command.InvitationId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command =>
                    $"Invitation does not have valid rowVersion! RowVersion={command.RowVersion}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> CurrentUserIsCreatorOrOfInvitationOrAdmin(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.CurrentUserIsCreatorOfInvitationOrAdminAsync(invitationId, cancellationToken);

            async Task<bool> InvitationIsCanceled(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
