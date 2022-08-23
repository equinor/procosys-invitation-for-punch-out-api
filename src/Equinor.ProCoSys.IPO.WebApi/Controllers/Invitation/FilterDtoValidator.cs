using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FilterDtoValidator : AbstractValidator<FilterDto>
    {
        public FilterDtoValidator()
        {
            RuleFor(x => x)
                .NotNull();
        }
    }
}
