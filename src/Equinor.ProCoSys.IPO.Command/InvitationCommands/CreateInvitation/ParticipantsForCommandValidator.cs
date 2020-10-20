using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class ParticipantsForCommandValidator : AbstractValidator<ParticipantsForCommand>
    {
        public ParticipantsForCommandValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(command => command)
                .Must((command) => command.SortKey >= 0)
                .WithMessage(command =>
                    $"Sort key must be a non negative integer! SortKey={command.SortKey}");
        }
    }
}
