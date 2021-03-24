using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UnCompletePunchOut
{
    public class UnCompletePunchOutCommandValidator : AbstractValidator<UnCompletePunchOutCommand>
    {
        public UnCompletePunchOutCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeAnInvitationInCompletedStage(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation is not in completed stage, and thus cannot be uncompleted!")
                .Must(command => HaveAValidRowVersion(command.InvitationRowVersion))
                .WithMessage(command =>
                    $"Invitation row version is not valid! InvitationRowVersion={command.InvitationRowVersion}")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, cancellationToken) => BeAContractorOnIpo(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "The IPO does not have a contractor assigned to uncomplete the IPO!")
                .MustAsync((command, cancellationToken) => BeThePersonWhoCompleted(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Person trying to uncomplete is not the person who completed the IPO!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAnInvitationInCompletedStage(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Completed, cancellationToken);

            async Task<bool> BeAContractorOnIpo(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ContractorExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeThePersonWhoCompleted(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.SameUserUnCompletingThatCompletedAsync(invitationId, cancellationToken);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
