using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class PersonForCommandValidator : AbstractValidator<PersonForCommand>
    {
        public PersonForCommandValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .Must((command) => 
                    command.FirstName != null && 
                    command.FirstName.Length > 1 && 
                    command.FirstName.Length < Participant.FirstNameMaxLength)
                .WithMessage(command =>
                    $"First name must be between 1 and {Participant.FirstNameMaxLength} characters! FirstName={command.FirstName}")
                .Must((command) =>
                    command.LastName != null &&
                    command.LastName.Length > 1 &&
                    command.LastName.Length < Participant.LastNameMaxLength)
                .WithMessage(command =>
                    $"Last name must be between 1 and {Participant.LastNameMaxLength} characters! LastName={command.LastName}");
        }
    }
}
