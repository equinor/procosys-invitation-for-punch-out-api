using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FunctionalRoleDtoValidator : AbstractValidator<FunctionalRoleDto>
    {
        public FunctionalRoleDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotNull()
                .MinimumLength(1)
                .MaximumLength(Participant.FunctionalRoleCodeMaxLength)
                .WithMessage("Functional role code must be between 1 and " + Participant.FunctionalRoleCodeMaxLength + " characters");
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}
