using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class DeleteAttachmentDtoValidator : AbstractValidator<DeleteAttachmentDto>
    {
        public DeleteAttachmentDtoValidator()
        {
            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
