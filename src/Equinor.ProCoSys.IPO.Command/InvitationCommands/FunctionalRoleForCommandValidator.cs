using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class FunctionalRoleForCommandValidator : AbstractValidator<FunctionalRoleForCommand>
    {
        public FunctionalRoleForCommandValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .Must(command => 
                    command.Code != null && 
                    command.Code.Length > 2 &&
                    command.Code.Length < Participant.FunctionalRoleCodeMaxLength)
                .WithMessage(command =>
                    $"Functional role code must be between 3 and {Participant.FunctionalRoleCodeMaxLength} characters! Code={command.Code}");
        }
    }
}
