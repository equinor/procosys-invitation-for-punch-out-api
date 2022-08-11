using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditParticipants
{
    public class EditParticipantsCommandValidator : AbstractValidator<EditParticipantsCommand>
    {
        public EditParticipantsCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                //input validators
                .Must(command => command.UpdatedParticipants != null && command.UpdatedParticipants.Any())
                .WithMessage("Participants must be invited!")

                //business validators
                .MustAsync((command, cancellationToken) => BeAnExistingIpo(command.InvitationId, cancellationToken))
                .WithMessage(command => $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .Must(command => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.UpdatedParticipants))
                .WithMessage("Contractor and Construction Company must be invited!")
                .Must(command => RequiredParticipantsHaveLowestSortKeys(command.UpdatedParticipants))
                .WithMessage("Contractor must be first and Construction Company must be second!")
                .Must(command => ParticipantListMustBeValid(command.UpdatedParticipants))
                .WithMessage("Each participant must contain an oid!")
                .MustAsync((command, cancellationToken) =>
                    NotBeCancelled(command.InvitationId, cancellationToken))
                .WithMessage(command => $"IPO cannot be cancelled when editing as admin! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) =>
                    SignedParticipantsCannotBeAltered(command.UpdatedParticipants, command.InvitationId,
                        cancellationToken))
                .WithMessage(command =>
                    $"Participants that have signed must be unsigned before edited! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) =>
                    SortKeyIsNotChangedForSignedFirstSigners(command.UpdatedParticipants, command.InvitationId,
                        cancellationToken))
                .WithMessage(command =>
                    $"Cannot change first contractor or construction company if they have signed! Id={command.InvitationId}"); ;

            RuleForEach(command => command.UpdatedParticipants)
                .MustAsync((command, participant, _, cancellationToken) => ParticipantToBeUpdatedMustExist(participant, command.InvitationId, cancellationToken))
                .WithMessage(_ => $"Participant with ID does not exist on invitation!")
                .Must(participant => participant.SortKey >= 0)
                .WithMessage((_, participant) =>
                    $"Sort key for participant must be a non negative number! SortKey={participant.SortKey}")
                .Must(FunctionalRoleParticipantsMustBeValid)
                .WithMessage((_, participant) =>
                    $"Functional role code must be between 3 and {Participant.FunctionalRoleCodeMaxLength} characters! Code={participant.InvitedFunctionalRole.Code}")
                .Must((_, participant) => ParticipantsHaveValidRowVersions(participant))
                .WithMessage(_ => "Participant doesn't have valid rowVersion!");


            async Task<bool> BeAnExistingIpo(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> NotBeCancelled(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);

            async Task<bool> ParticipantToBeUpdatedMustExist(ParticipantsForCommand participant, int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.ParticipantWithIdExistsAsync(participant, invitationId, cancellationToken);

            async Task<bool> SignedParticipantsCannotBeAltered(IList<ParticipantsForEditCommand> participants, int invitationId,
                CancellationToken cancellationToken)
                => await invitationValidator.SignedParticipantsCannotBeAlteredAsync(participants, invitationId, cancellationToken);

            async Task<bool> SortKeyIsNotChangedForSignedFirstSigners(IList<ParticipantsForEditCommand> participants, int invitationId,
                CancellationToken cancellationToken)
                => await invitationValidator.SortKeyCannotBeChangedForSignedFirstSignersAsync(participants, invitationId, cancellationToken);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForEditCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants.Cast<ParticipantsForCommand>().ToList());

            bool RequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForEditCommand> participants)
                => invitationValidator.OnlyRequiredParticipantsHaveLowestSortKeys(participants.Cast<ParticipantsForCommand>().ToList());

            bool ParticipantListMustBeValid(IList<ParticipantsForEditCommand> participants)
                => invitationValidator.IsValidParticipantList(participants.Cast<ParticipantsForCommand>().ToList());

            bool ParticipantsHaveValidRowVersions(ParticipantsForEditCommand participant)
            {
                if (participant.InvitedExternalEmailToEdit?.Id != null)
                {
                    return rowVersionValidator.IsValid(participant.InvitedExternalEmailToEdit.RowVersion);
                }
                if (participant.InvitedPersonToEdit?.Id != null)
                {
                    return rowVersionValidator.IsValid(participant.InvitedPersonToEdit.RowVersion);
                }

                if (participant.InvitedFunctionalRoleToEdit != null)
                {
                    if (participant.InvitedFunctionalRoleToEdit.Id != null && !rowVersionValidator.IsValid(participant.InvitedFunctionalRoleToEdit.RowVersion))
                    {
                        return false;
                    }

                    return participant.InvitedFunctionalRoleToEdit.EditPersons.All(person => person.Id == null || rowVersionValidator.IsValid(person.RowVersion));
                }

                return true;
            }

            bool FunctionalRoleParticipantsMustBeValid(ParticipantsForCommand participant)
            {
                if (participant.InvitedFunctionalRole == null)
                {
                    return true;
                }

                return participant.InvitedFunctionalRole.Code != null &&
                    participant.InvitedFunctionalRole.Code.Length > 2 &&
                    participant.InvitedFunctionalRole.Code.Length < Participant.FunctionalRoleCodeMaxLength;
            }
        }
    }
}
