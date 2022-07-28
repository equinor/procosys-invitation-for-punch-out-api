using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommandValidator : AbstractValidator<EditInvitationCommand>
    {
        public EditInvitationCommandValidator(IInvitationValidator invitationValidator, IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                //input validators
                .Must(command => command.UpdatedParticipants != null)
                .WithMessage("Participants cannot be null!")
                .Must(command => command.Description == null || command.Description.Length < Invitation.DescriptionMaxLength)
                .WithMessage(command =>
                    $"Description cannot be more than {Invitation.DescriptionMaxLength} characters! Description={command.Description}")
                .Must(command => command.Location == null || command.Location.Length < Invitation.LocationMaxLength)
                .WithMessage(command =>
                    $"Location cannot be more than {Invitation.LocationMaxLength} characters! Location={command.Location}")
                .Must(command => command.StartTime < command.EndTime)
                .WithMessage(command =>
                    $"Start time must be before end time! Start={command.StartTime} End={command.EndTime}")
                .Must(command =>
                    command.Title != null &&
                    command.Title.Length >= Invitation.TitleMinLength &&
                    command.Title.Length < Invitation.TitleMaxLength)
                .WithMessage(command =>
                    $"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters! Title={command.Title}")

                //business validators
                .MustAsync((command, cancellationToken) => BeAnExistingIpo(command.InvitationId, cancellationToken))
                .WithMessage(command => $"Invitation with this ID does not exist! Id={command.InvitationId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command => $"Invitation does not have valid rowVersion! RowVersion={command.RowVersion}")
                .Must(command => MustHaveValidScope(command.Type, command.UpdatedMcPkgScope, command.UpdatedCommPkgScope))
                .WithMessage("Not a valid scope! Choose either DP with mc scope or MDP with comm pkg scope")
                .Must(command => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.UpdatedParticipants))
                .WithMessage("Contractor and Construction Company must be invited!")
                .Must(command => RequiredParticipantsHaveLowestSortKeys(command.UpdatedParticipants))
                .WithMessage("Contractor must be first and Construction Company must be second!")
                .Must(command => ParticipantListMustBeValid(command.UpdatedParticipants))
                .WithMessage("Each participant must contain an email or oid!");

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

            WhenAsync((_, _) => IsAdmin(), () =>
            {
                RuleFor(command => command)
                    .MustAsync((command, cancellationToken) =>
                        NotBeCancelled(command.InvitationId, cancellationToken))
                    .WithMessage(command => $"IPO cannot be cancelled when editing as admin! Id={command.InvitationId}")
                    .MustAsync((command, cancellationToken) =>
                        SignedParticipantsCannotBeAltered(command.UpdatedParticipants, command.InvitationId, cancellationToken))
                    .WithMessage(command => $"Participants that have signed must be unsigned before edited! Id={command.InvitationId}")
                    .MustAsync((command, cancellationToken) =>
                        SortKeyIsNotChangedForSignedFirstSigners(command.UpdatedParticipants, command.InvitationId, cancellationToken))
                    .WithMessage(command => $"Cannot change first contractor or construction company if they have signed! Id={command.InvitationId}");
            }).Otherwise(() =>
            {
                RuleFor(command => command)
                    .MustAsync((command, cancellationToken) =>
                        BeAnIpoInPlannedStage(command.InvitationId, cancellationToken))
                    .WithMessage(command => $"IPO must be in planned stage to be edited! Id={command.InvitationId}");
            });

            async Task<bool> BeAnExistingIpo(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeAnIpoInPlannedStage(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Planned, cancellationToken);

            async Task<bool> NotBeCancelled(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);

            bool MustHaveValidScope(
                DisciplineType type,
                IList<string> updatedMcPkgScope, 
                IList<string> updatedCommPkgScope) 
                => invitationValidator.IsValidScope(type, updatedMcPkgScope, updatedCommPkgScope);

            async Task<bool> IsAdmin() => await invitationValidator.CurrentUserIsAdmin();

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

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);

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
