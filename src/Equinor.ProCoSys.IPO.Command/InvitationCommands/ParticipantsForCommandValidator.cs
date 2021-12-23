﻿using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    // todo Dead code. This validator will never be hit. Move to CreateInvitationCommandValidator and EditInvitationCommandValidator
    public class ParticipantsForCommandValidator : AbstractValidator<ParticipantsForCommand>
    {
        public ParticipantsForCommandValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .Must(command => command.SortKey >= 0)
                .WithMessage(command =>
                    $"Sort key must be a non negative integer! SortKey={command.SortKey}");
        }
    }
}
