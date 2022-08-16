using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson
{
    public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
    {
        public CreatePersonCommandValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(Person.EmailLengthMax);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(Person.FirstNameLengthMax);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(Person.LastNameLengthMax);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(Person.UserNameLengthMax);
        }
    }
}
