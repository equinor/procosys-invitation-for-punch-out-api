using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class ParticipantDtoValidator : AbstractValidator<ParticipantDto>
    {
        public ParticipantDtoValidator() =>
            RuleFor(x => x.SortKey)
                .NotNull();
    }
}
