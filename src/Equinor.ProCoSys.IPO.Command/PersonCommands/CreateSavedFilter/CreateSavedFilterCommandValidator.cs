using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter
{
    public class CreateSavedFilterCommandValidator : AbstractValidator<CreateSavedFilterCommand>
    {
        public CreateSavedFilterCommandValidator(
            ISavedFilterValidator savedFilterValidator)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => NotExistsASavedFilterWithSameTitleForPersonOnProject(command.Title, command.ProjectName, cancellationToken))
                .WithMessage(command => $"A saved filter with this title already exists! Title={command.Title}");

            async Task<bool> NotExistsASavedFilterWithSameTitleForPersonOnProject(string title, string projectName, CancellationToken cancellationToken)
                => !await savedFilterValidator.ExistsWithSameTitleForPersonInProjectAsync(title, projectName, cancellationToken);
        }
    }
}
