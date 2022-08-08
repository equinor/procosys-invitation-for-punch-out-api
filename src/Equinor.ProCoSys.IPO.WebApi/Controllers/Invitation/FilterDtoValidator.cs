using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FilterDtoValidator : AbstractValidator<FilterDto>
    {
        public FilterDtoValidator()
        {
            RuleFor(x => x)
                .NotNull();

            RuleFor(x => x.ProjectName)
                .NotNull()
                .NotEmpty();
        }
    }
}
