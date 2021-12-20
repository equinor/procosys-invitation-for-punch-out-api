using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.DeleteSavedFilter
{
    public class DeleteSavedFilterCommandValidator : AbstractValidator<DeleteSavedFilterCommand>
    {
        public DeleteSavedFilterCommandValidator(
            ISavedFilterValidator savedFilterValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingSavedFilterAsync(command.SavedFilterId, cancellationToken))
                .WithMessage(command => $"Saved filter with this ID does not exist! Id={command.SavedFilterId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command => $"Not a valid row version! Row version={command.RowVersion}");

            async Task<bool> BeAnExistingSavedFilterAsync(int savedFilterId, CancellationToken cancellationToken)
                => await savedFilterValidator.ExistsAsync(savedFilterId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
