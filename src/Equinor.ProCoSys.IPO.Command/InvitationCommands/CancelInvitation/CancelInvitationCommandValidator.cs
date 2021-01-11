using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelInvitation
{
    public class CancelInvitationCommandValidator : AbstractValidator<CancelInvitationCommand>
    {
        public CancelInvitationCommandValidator(IInvitationValidator invitationValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => CurrentUserIsCreatorOfInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"Current user is not the creator of the invitation! Id={command.InvitationId}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> CurrentUserIsCreatorOfInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.CurrentUserIsCreatorOfInvitation(invitationId, token);
        }
    }
}
