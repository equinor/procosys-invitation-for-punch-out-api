﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants
{
    public class UpdateAttendedStatusAndNotesOnParticipantsCommandValidator : AbstractValidator<UpdateAttendedStatusAndNotesOnParticipantsCommand>
    {
        public UpdateAttendedStatusAndNotesOnParticipantsCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingInvitation(command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => BeAnInvitationInCompletedStage(command.InvitationId, token))
                .WithMessage(command =>
                    "Invitation is not in completed stage, and thus cannot change attended statuses or notes!")
                .MustAsync((command, token) => BeAContractorOnIpo(command.InvitationId, token))
                .WithMessage(command =>
                    "The IPO does not have a contractor assigned to the IPO!")
                .MustAsync((command, token) => BeTheAssignedContractorIfPersonParticipant(command.InvitationId, token))
                .WithMessage(command =>
                    "User is not the contractor assigned to complete this IPO!");

            RuleForEach(command => command.Participants)
                .MustAsync((command, participant, _, token) => BeAnExistingParticipant(participant.Id, command.InvitationId, token))
                .WithMessage((command, participant) =>
                    $"Participant with ID does not exist on invitation! ParticipantId={participant.Id}")
                .Must((command, participant) => HaveAValidRowVersion(participant.RowVersion))
                .WithMessage((command, participant) =>
                    $"Participant doesn't have valid rowVersion! ParticipantRowVersion={participant.RowVersion}");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> BeAnInvitationInCompletedStage(int invitationId, CancellationToken token)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Completed, token);

            async Task<bool> BeAnExistingParticipant(int participantId, int invitationId, CancellationToken token)
                => await invitationValidator.ParticipantExists(participantId, invitationId, token);

            async Task<bool> BeTheAssignedContractorIfPersonParticipant(int invitationId, CancellationToken token)
                => await invitationValidator.ValidContractorParticipantExistsAsync(invitationId, token);

            async Task<bool> BeAContractorOnIpo(int invitationId, CancellationToken token)
                => await invitationValidator.ContractorExistsAsync(invitationId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}
