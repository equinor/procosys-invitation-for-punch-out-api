using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class CreateInvitationDtoValidator : AbstractValidator<CreateInvitationDto>
    {
        public CreateInvitationDtoValidator()
        {
            RuleFor(x => x.Participants)
                .NotNull();
            RuleFor(x => x.Type).NotNull();
            RuleFor(x => x.ProjectName)
                .NotNull()
                .MinimumLength(3)
                .WithMessage("Project name must be at least 3 characters");
            RuleFor(x => x.Description)
                .MaximumLength(4000)
                .WithMessage("Description can be max 4000 characters");
            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
            RuleFor(x => x.Location).MaximumLength(1024);
            RuleFor(x => x.Title)
                .NotNull()
                .MinimumLength(3)
                .MaximumLength(1024)
                .WithMessage("Title must be between 3 and 1024 characters");
        }
    }
}
