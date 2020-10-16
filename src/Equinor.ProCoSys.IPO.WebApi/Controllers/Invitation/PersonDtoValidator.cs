using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class PersonDtoValidator : AbstractValidator<PersonDto>
    {
        public PersonDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotNull()
                .MinimumLength(1)
                .MaximumLength(Participant.FirstNameMaxLength)
                .WithMessage("First name must be between 1 and " + Participant.FirstNameMaxLength + " characters");
            RuleFor(x => x.LastName)
                .NotNull()
                .MinimumLength(1)
                .MaximumLength(Participant.LastNameMaxLength)
                .WithMessage("Last name must be between 1 and " + Participant.LastNameMaxLength + " characters");
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}
