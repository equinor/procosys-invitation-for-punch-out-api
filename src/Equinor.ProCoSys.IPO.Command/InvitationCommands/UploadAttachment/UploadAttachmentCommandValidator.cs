using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment
{
    public class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
    {
        public UploadAttachmentCommandValidator(IInvitationValidator invitationValidator, IOptionsMonitor<BlobStorageOptions> options)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x)
                .NotNull();

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("Filename not given!")
                .MaximumLength(Attachment.FileNameLengthMax)
                .WithMessage($"Filename too long! Max {Attachment.FileNameLengthMax} characters")
                .Must(BeAValidFile)
                .WithMessage(x => $"File {x.FileName} is not a valid file for upload!");

            RuleFor(x => x.Content.Length)
                .Must(BeSmallerThanMaxSize)
                .When(x => x.Content != null)
                .WithMessage($"Maximum file size is {options.CurrentValue.MaxSizeMb}MB!");

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitationAsync(command.InvitationId, cancellationToken))
                    .WithMessage(command => $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => NotHaveAttachmentWithFileNameAsync(command.InvitationId, command.FileName, cancellationToken))
                    .WithMessage(command => $"Invitation already has an attachment with filename {command.FileName}! Please rename file or choose to overwrite.")
                    .When(c => !c.OverWriteIfExists, ApplyConditionTo.CurrentValidator);

            bool BeAValidFile(string fileName)
            {
                var suffix = Path.GetExtension(fileName?.ToLower());
                return suffix != null && !options.CurrentValue.BlockedFileSuffixes.Contains(suffix) && fileName?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            }

            bool BeSmallerThanMaxSize(long fileSizeInBytes)
            {
                var maxSizeInBytes = options.CurrentValue.MaxSizeMb * 1024 * 1024;
                return fileSizeInBytes < maxSizeInBytes;
            }

            async Task<bool> BeAnExistingInvitationAsync(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> NotHaveAttachmentWithFileNameAsync(int invitationId, string fileName, CancellationToken cancellationToken)
                => !await invitationValidator.AttachmentWithFileNameExistsAsync(invitationId, fileName, cancellationToken);
        }
    }
}
