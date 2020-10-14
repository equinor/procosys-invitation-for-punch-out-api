﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
    {
        public CreateInvitationCommandValidator(IInvitationValidator invitationValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(command => command)
                .MustAsync((command, token) => TitleMustBeUniqueOnProject(command.ProjectName, command.Title, token))
                .WithMessage(command =>
                    $"IPO with this title already exists in project! Title={command.Title}")
                .Must((command, token) => MustHaveValidScope(command.McPkgScope, command.CommPkgScope))
                .WithMessage(command =>
                    $"Scope must be valid. Either mc scope or comm pgk scope must be added, but not both!")
                .Must((command, token) => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.Participants))
                .WithMessage(command =>
                    $"Contractor and Construction Company must be invited!")
                .Must((command, token) => ParticipantMustContainEmail(command.Participants))
                .WithMessage(command =>
                    $"Each participant must contain an email!");

            async Task<bool> TitleMustBeUniqueOnProject(string projectName, string title, CancellationToken token)
                => !await invitationValidator.TitleExistsOnProjectAsync(projectName, title, token);

            bool MustHaveValidScope(IList<McPkgScopeForCommand> mcPkgScope, IList<CommPkgScopeForCommand> commPkgScope)
                => invitationValidator.IsValidScope(mcPkgScope, commPkgScope);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants);


            bool ParticipantMustContainEmail(IList<ParticipantsForCommand> participants)
                => invitationValidator.IsValidParticipantList(participants);
        }
    }
}
