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
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => BeAnInvitationInCompletedStage(command.InvitationId, token))
                .WithMessage(command =>
                    "Invitation is not in completed stage, and thus cannot be uncompleted!")
                .Must(command => HaveAValidRowVersion(command.InvitationRowVersion))
                .WithMessage(command =>
                    $"Invitation row version is not valid! InvitationRowVersion={command.InvitationRowVersion}")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, token) => BeAContractorOnIpo(command.InvitationId, token))
                .WithMessage(command =>
                    "The IPO does not have a contractor assigned to uncomplete the IPO!")
                .MustAsync((command, token) => BeThePersonWhoCompleted(command.InvitationId, token))
                .WithMessage(command =>
                    "Person trying to uncomplete is not the person who completed the IPO!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> BeAnInvitationInCompletedStage(int invitationId, CancellationToken token)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Completed, token);

            async Task<bool> BeAContractorOnIpo(int invitationId, CancellationToken token)
                => await invitationValidator.ContractorExistsAsync(invitationId, token);

            async Task<bool> BeThePersonWhoCompleted(int invitationId, CancellationToken token)
                => await invitationValidator.SameUserUnCompletingThatCompletedAsync(invitationId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
