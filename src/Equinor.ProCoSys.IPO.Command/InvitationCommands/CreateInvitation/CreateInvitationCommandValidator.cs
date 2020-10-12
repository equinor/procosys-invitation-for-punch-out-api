using System.Collections.Generic;
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
                //.MustAsync((command, token) => ProjectMustExist(command.ProjectName, token))
                //.WithMessage(command =>
                //    $"Project doesn't exist'! ProjectName={command.ProjectName}")
                .MustAsync((command, token) => TitleMustBeUniqueOnProject(command.ProjectName, command.Title, token))
                .WithMessage(command =>
                    $"IPO with this title already exists in project! Title={command.Title}")
                .Must((command, token) => MustHaveValidScope(command.McPkgScope, command.CommPkgScope))
                .WithMessage(command =>
                    $"Scope must be valid. Either mc scope or comm pgk scope must be added, but not both!");

                async Task<bool> ProjectMustExist(string projectName, CancellationToken token)
                    => await invitationValidator.ProjectExistsAsync(projectName, token);

                async Task<bool> TitleMustBeUniqueOnProject(string projectName, string title, CancellationToken token)
                    => !await invitationValidator.TitleExistsOnProjectAsync(projectName, title, token);

                bool MustHaveValidScope(IList<McPkgScopeForCommand> mcPkgScope, IList<CommPkgScopeForCommand> commPkgScope)
                    => invitationValidator.IsValidScope(mcPkgScope, commPkgScope);
            }
    }
}
