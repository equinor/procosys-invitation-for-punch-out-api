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
                .Must((command) => command.UpdatedParticipants != null)
                .WithMessage(command =>
                    "Participants cannot be null!")
                .Must((command) => command.Description == null || command.Description.Length < 4000)
                .WithMessage(command =>
                    $"Description cannot be more than 4000 characters! Description={command.Description}")
                .Must((command) => command.StartTime < command.EndTime)
                .WithMessage(command =>
                    $"Start time must be before end time! Start={command.StartTime} End={command.EndTime}")
                .Must((command) =>
                    command.Title != null &&
                    command.Title.Length > 2 &&
                    command.Title.Length < Invitation.TitleMaxLength)
                .WithMessage(command =>
                    $"Title must be between 3 and 1024 characters! Title={command.Title}")
                .Must((command) => command.Location == null || command.Location.Length < 1024)
                .WithMessage(command =>
                    $"Location cannot be more than 1024 characters! Location={command.Location}")
                //business validators
                .MustAsync((command, token) => BeAnExistingIpo(command.InvitationId, token))
                .WithMessage(command => $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, token) => BeAnIpoInPlannedStage(command.InvitationId, token))
                .WithMessage(command => $"IPO must be in planned stage to be edited! Id={command.InvitationId}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command => $"Invitation does not have valid rowVersion! RowVersion={command.RowVersion}")
                .Must((command) => MustHaveValidScope(command.UpdatedMcPkgScope, command.UpdatedCommPkgScope))
                .WithMessage(command =>
                    "Not a valid scope! Choose either mc scope or comm pkg scope")
                .Must((command) => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.UpdatedParticipants))
                .WithMessage(command =>
                    "Contractor and Construction Company must be invited!")
                .Must((command) => RequiredParticipantsHaveLowestSortKeys(command.UpdatedParticipants))
                .WithMessage(command =>
                    "SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company!")
                .Must((command) => ParticipantListMustBeValid(command.UpdatedParticipants))
                .WithMessage(command =>
                    "Each participant must contain an email or oid!");

            RuleForEach(command => command.UpdatedParticipants)
                .MustAsync((command, participant, _, token) => ParticipantToBeUpdatedMustExist(participant, command.InvitationId, token))
                .WithMessage((command, participant) =>
                    $"Participant with ID does not exist on invitation! Participant={participant}")
                .Must((command, participant) => ParticipantsHaveValidRowVersions(participant))
                .WithMessage((command, participant) =>
                    $"Participant doesn't have valid rowVersion! Participant={participant}");

            async Task<bool> BeAnExistingIpo(int invitationId, CancellationToken token)
                => await invitationValidator.IpoExistsAsync(invitationId, token);

            async Task<bool> BeAnIpoInPlannedStage(int invitationId, CancellationToken token)
                => await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Planned, token);

            bool MustHaveValidScope(
                IList<string> updatedMcPkgScope, 
                IList<string> updatedCommPkgScope) 
                => invitationValidator.IsValidScope(updatedMcPkgScope, updatedCommPkgScope);

            async Task<bool> ParticipantToBeUpdatedMustExist(ParticipantsForCommand participant, int invitationId, CancellationToken token)
                => await invitationValidator.ParticipantWithIdExistsAsync(participant, invitationId, token);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants);

            bool RequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
                => invitationValidator.OnlyRequiredParticipantsHaveLowestSortKeys(participants);

            bool ParticipantListMustBeValid(IList<ParticipantsForCommand> participants)
                => invitationValidator.IsValidParticipantList(participants);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);

            bool ParticipantsHaveValidRowVersions(ParticipantsForCommand participant)
            {
                if (participant.ExternalEmail?.Id != null)
                {
                    return rowVersionValidator.IsValid(participant.ExternalEmail.RowVersion);
                }
                if (participant.Person?.Id != null)
                {
                    return rowVersionValidator.IsValid(participant.Person.RowVersion);
                }

                if (participant.FunctionalRole != null)
                {
                    if (participant.FunctionalRole.Id != null && !rowVersionValidator.IsValid(participant.FunctionalRole.RowVersion))
                    {
                        return false;
                    }

                    return participant.FunctionalRole.Persons.All(person => person.Id == null || rowVersionValidator.IsValid(person.RowVersion));
                }

                return true;
            }
        }
    }
}
