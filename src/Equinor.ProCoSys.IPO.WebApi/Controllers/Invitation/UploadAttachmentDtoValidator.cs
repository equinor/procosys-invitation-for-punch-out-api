using System;
using System.Linq;
using Equinor.ProCoSys.IPO.BlobStorage;
using FluentValidation;
using System.IO;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class UploadAttachmentDtoValidator : AbstractValidator<UploadAttachmentDto>
    {
        public UploadAttachmentDtoValidator(IOptionsMonitor<BlobStorageOptions> options)
        {
            RuleFor(x => x)
                .NotNull();

            RuleFor(x => x.File)
                .NotNull();

            RuleFor(x => x.File.FileName)
                .NotEmpty()
                .WithMessage("Filename not given!")
                .MaximumLength(Attachment.FileNameLengthMax)
                .WithMessage($"Filename to long! Max {Attachment.FileNameLengthMax} characters")
                .Must(BeAValidFile)
                .WithMessage(x => $"File {x.File.FileName} is not a valid file for upload!")
                .When(x => x.File != null);

            RuleFor(x => x.File.Length)
                .Must(BeSmallerThanMaxSize)
                .When(x => x.File != null)
                .WithMessage($"Maximum file size is {options.CurrentValue.MaxSizeMb}MB!");

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
        }
    }
}
