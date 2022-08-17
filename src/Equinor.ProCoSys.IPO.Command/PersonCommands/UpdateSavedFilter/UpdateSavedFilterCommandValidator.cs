using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.UpdateSavedFilter
{
    public class UpdateSavedFilterCommandValidator : AbstractValidator<UpdateSavedFilterCommand>
    {
         public UpdateSavedFilterCommandValidator(
            ISavedFilterValidator savedFilterValidator,
            IRowVersionValidator rowVersionValidator)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingSavedFilterAsync(command.SavedFilterId, cancellationToken))
                .WithMessage(command => $"Saved filter with this ID does not exist! Id={command.SavedFilterId}")
                .MustAsync((command, cancellationToken) => HaveAUniqueTitleForPerson(command.Title, command.SavedFilterId, cancellationToken))
                .WithMessage(command => $"A saved filter with this title already exists! Title={command.Title}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command => $"Not a valid row version! Row version={command.RowVersion}");

            async Task<bool> BeAnExistingSavedFilterAsync(int savedFilterId, CancellationToken cancellationToken)
                => await savedFilterValidator.ExistsAsync(savedFilterId, cancellationToken);
            async Task<bool> HaveAUniqueTitleForPerson(string title, int savedFilterId, CancellationToken cancellationToken)
                => !await savedFilterValidator.ExistsAnotherWithSameTitleForPersonInProjectAsync(savedFilterId, title, cancellationToken);
            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
