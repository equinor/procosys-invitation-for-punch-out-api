using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.AttachmentValidators;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment
{
    public class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
    {
        public DeleteAttachmentCommandValidator(
            IInvitationValidator invitationValidator,
            IAttachmentValidator attachmentValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitationAsync(command.InvitationId, token))
                    .WithMessage(command => $"Invitation doesn't exist. Invitation={command.InvitationId}.")
                .MustAsync((command, token) => BeAnExistingAttachmentAsync(command.AttachmentId, token))
                    .WithMessage(command => $"Attachment doesn't exist. Attachment={command.AttachmentId}.")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                    .WithMessage(command => $"Not a valid row version. Row version={command.RowVersion}."); ;

            async Task<bool> BeAnExistingInvitationAsync(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ExistsAsync(invitationId, cancellationToken);
            async Task<bool> BeAnExistingAttachmentAsync(int attachmentId, CancellationToken token)
                => await attachmentValidator.ExistsAsync(attachmentId, token);
            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
