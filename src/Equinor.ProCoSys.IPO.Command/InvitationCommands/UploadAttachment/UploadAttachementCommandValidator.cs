using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment
{
    public class UploadAttachementCommandValidator : AbstractValidator<UploadAttachmentCommand>
    {
        public UploadAttachementCommandValidator(IInvitationValidator invitationValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitationAsync(command.InvitationId, token))
                    .WithMessage(command => $"Invitation doesn't exist! Invitation={command.InvitationId}")
                .MustAsync((command, token) => NotHaveAttachmentWithFileNameAsync(command.InvitationId, command.FileName, token))
                    .WithMessage(command => $"Invitation already has an attachment with filename {command.FileName}! Please rename file or choose to overwrite.")
                    .When(c => !c.OverWriteIfExists, ApplyConditionTo.CurrentValidator);

            async Task<bool> BeAnExistingInvitationAsync(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ExistsAsync(invitationId, cancellationToken);
            async Task<bool> NotHaveAttachmentWithFileNameAsync(int invitationId, string fileName, CancellationToken token)
                => !await invitationValidator.AttachmentWithFileNameExistsAsync(invitationId, fileName, token);
        }
    }
}
