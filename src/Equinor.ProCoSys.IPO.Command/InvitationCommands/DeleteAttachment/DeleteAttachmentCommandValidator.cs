using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment
{
    public class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
    {
        public DeleteAttachmentCommandValidator(
            IInvitationValidator invitationValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitationAsync(command.InvitationId, cancellationToken))
                    .WithMessage(command => $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeAnExistingAttachmentAsync(command.InvitationId, command.AttachmentId, cancellationToken))
                    .WithMessage(command => $"Attachment doesn't exist! Attachment={command.AttachmentId}.")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                    .WithMessage(command => $"Not a valid row version! Row version={command.RowVersion}.");

            async Task<bool> BeAnExistingInvitationAsync(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);
            async Task<bool> BeAnExistingAttachmentAsync(int invitationId, int attachmentId, CancellationToken cancellationToken)
                => await invitationValidator.AttachmentExistsAsync(invitationId, attachmentId, cancellationToken);
            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
