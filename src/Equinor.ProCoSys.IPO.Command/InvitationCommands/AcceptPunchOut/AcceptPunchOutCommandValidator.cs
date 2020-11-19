using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut
{
    public class AcceptPunchOutCommandValidator : AbstractValidator<AcceptPunchOutCommand>
    {
        public AcceptPunchOutCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => BeAnInvitationInCompletedStage(command.InvitationId, token))
                .WithMessage(command =>
                    "Invitation is not in completed stage, and thus cannot be accepted!")
                .Must((command) => HaveAValidRowVersion(command.InvitationRowVersion))
                .WithMessage(command =>
                    $"Invitation row version is not valid! InvitationRowVersion={command.InvitationRowVersion}")
                .Must(command => HaveAValidRowVersion(command.ParticipantRowVersion))
                .WithMessage(command =>
                    $"Participant row version is not valid! ParticipantRowVersion={command.ParticipantRowVersion}")
                .MustAsync((command, token) => BeAConstructionCompanyOnIpo(command.InvitationId, token))
                .WithMessage(command =>
                    "The IPO does not have a construction company assigned to accept the IPO!")
                .MustAsync((command, token) => BeTheAssignedPersonIfPersonParticipant(command.InvitationId, token))
                .WithMessage(command =>
                    "Person signing is not the construction company assigned to accept this IPO, or there is not a valid construction company on the IPO!");

            RuleForEach(command => command.Participants)
                .MustAsync((command, participant, _, token) => BeAnExistingParticipant(participant.Id, command.InvitationId, token))
                .WithMessage((command, participant) =>
                    $"Participant with ID does not exist on invitation! Participant={participant}")
                .Must((command, participant) => HaveAValidRowVersion(participant.RowVersion))
                .WithMessage((command, participant) =>
                    $"Participant doesn't have valid rowVersion! Participant={participant}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> BeAnInvitationInCompletedStage(int invitationId, CancellationToken token)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Completed, token);

            async Task<bool> BeAConstructionCompanyOnIpo(int invitationId, CancellationToken token)
                => await invitationValidator.ConstructionCompanyExistsAsync(invitationId, token);

            async Task<bool> BeTheAssignedPersonIfPersonParticipant(int invitationId, CancellationToken token)
                => await invitationValidator.ValidConstructionCompanyParticipantExistsAsync(invitationId, token);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken token)
                => await invitationValidator.ParticipantExists(participantId, invitationId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
