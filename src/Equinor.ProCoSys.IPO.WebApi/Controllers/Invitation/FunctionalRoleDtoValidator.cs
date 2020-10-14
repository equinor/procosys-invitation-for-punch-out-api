using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FunctionalRoleDtoValidator : AbstractValidator<FunctionalRoleDto>
    {
        public FunctionalRoleDtoValidator() => RuleFor(x => x.Code).NotNull();
    }
}
