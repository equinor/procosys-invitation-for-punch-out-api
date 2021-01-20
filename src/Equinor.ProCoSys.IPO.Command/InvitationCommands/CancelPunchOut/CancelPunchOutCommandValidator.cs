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
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => InvitationIsNotCanceled(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO is already canceled! Id={command.InvitationId}")
                .MustAsync((command, token) => InvitationIsNotAccepted(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO is in accepted stage! Id={command.InvitationId}")
                .MustAsync((command, token) => CurrentUserIsCreatorOfInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"Current user is not the creator of the invitation! Id={command.InvitationId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command =>
                    $"Invitation does not have valid rowVersion! RowVersion={command.RowVersion}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> CurrentUserIsCreatorOfInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.CurrentUserIsCreatorOfInvitation(invitationId, token);

            async Task<bool> InvitationIsNotCanceled(int invitationId, CancellationToken token)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, token);

            async Task<bool> InvitationIsNotAccepted(int invitationId, CancellationToken token)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Accepted, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
