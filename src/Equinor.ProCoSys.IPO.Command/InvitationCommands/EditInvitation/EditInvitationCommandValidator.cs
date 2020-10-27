using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommandValidator : AbstractValidator<EditInvitationCommand>
    {
        public EditInvitationCommandValidator(IInvitationValidator invitationValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleForEach(command => command.UpdatedParticipants)
                .Must((command, participant) => ParticipantMustHaveId(participant))
                .WithMessage((command, participant) =>
                    $"Participant doesn't have id! Participant={participant}")
                .MustAsync((command, participant, _, token) => ParticipantToBeUpdatedMustExist(participant, token))
                .WithMessage((command, participant) =>
                    $"Participant doesn't exist! Participant={participant}");

            RuleFor(command => command)
                .Must((command) => command.UpdatedParticipants != null)
                .WithMessage(command =>
                    $"Participants cannot be null!")
                .Must((command) =>
                    command.ProjectName != null &&
                    command.ProjectName.Length > 2 &&
                    command.ProjectName.Length < Invitation.ProjectNameMaxLength)
                .WithMessage(command =>
                    $"Project name must be between 3 and {Invitation.ProjectNameMaxLength} characters! ProjectName={command.ProjectName}")
                .MustAsync((command, token) => ProjectMustNotBeChanged(command.ProjectName, command.InvitationId, token))
                .WithMessage(command =>
                    $"Project name cannot be changed! ProjectName={command.ProjectName}")
                .Must((command) => command.Description == null || command.Description.Length < 4000)
                .WithMessage(command =>
                    $"Description cannot be more than 4000 characters! Description={command.Description}")
                .Must((command) => command.StartTime < command.EndTime) //TODO: should there be a check that start time is a time in the future? Change for create IPO as well if so
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
                .MustAsync((command, token) => TitleMustBeUniqueOnProject(command.ProjectName, command.Title, command.InvitationId, token))
                .WithMessage(command =>
                    $"IPO with this title already exists in project! Title={command.Title}")
                .Must((command) => MustHaveValidScope(command.UpdatedMcPkgScope, command.NewMcPkgScope, command.UpdatedCommPkgScope, command.NewCommPkgScope))
                .WithMessage(command =>
                    $"Not a valid scope! Choose either mc scope or comm pkg scope")
                .Must((command) => McScopeMustBeUnderSameCommPkg(command.UpdatedMcPkgScope, command.NewMcPkgScope)) //TODO skriv tester på dette
                .WithMessage(command =>
                    $"Not a valid scope! All mc packages must be under same comm pkg")
                .Must((command) => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.UpdatedParticipants))
                .WithMessage(command =>
                    $"Contractor and Construction Company must be invited!")
                .Must((command) => RequiredParticipantsHaveLowestSortKeys(command.UpdatedParticipants))
                .WithMessage(command =>
                    $"SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company!")
                .Must((command) => NewParticipantsCannotHaveLowestSortKeys(command.NewParticipants))
                .WithMessage(command =>
                    $"SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company!")
                .Must((command) => ParticipantListMustBeValid(command.UpdatedParticipants))
                .WithMessage(command =>
                    $"Each participant must contain an email or oid!")
                .Must((command) => ParticipantListMustBeValid(command.NewParticipants))
                .WithMessage(command =>
                    $"Each participant must contain an email or oid!");

            async Task<bool> TitleMustBeUniqueOnProject(string projectName, string title, int id, CancellationToken token)
                => !await invitationValidator.IpoTitleExistsInProjectOnAnotherIpoAsync(projectName, title, id, token);

            bool MustHaveValidScope(
                IList<McPkgScopeForCommand> updatedMcPkgScope, 
                IList<McPkgScopeForCommand> newMcPkgScope,
                IList<CommPkgScopeForCommand> updatedCommPkgScope,
                IList<CommPkgScopeForCommand> newCommPkgScope)
            {
                var mcPkgScope = updatedMcPkgScope.Concat(newMcPkgScope);
                var commPkgScope = updatedCommPkgScope.Concat(newCommPkgScope);

                return invitationValidator.IsValidScope(mcPkgScope.ToList(), commPkgScope.ToList());
            }

            bool McScopeMustBeUnderSameCommPkg(IList<McPkgScopeForCommand> updatedMcPkgScope, IList<McPkgScopeForCommand> newMcPkgScope)
            {
                var mcPkgScope = updatedMcPkgScope.Concat(newMcPkgScope);
                return invitationValidator.McScopeIsUnderSameCommPkg(mcPkgScope.ToList());
            }

            async Task<bool> ParticipantToBeUpdatedMustExist(ParticipantsForCommand participant, CancellationToken token)
                => await invitationValidator.ParticipantExistsAsync(participant, token);

            async Task<bool> ProjectMustNotBeChanged(string projectName, int id, CancellationToken token)
                => await invitationValidator.ProjectNameIsNotChangedAsync(projectName, id, token);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants);

            bool RequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
                => invitationValidator.OnlyRequiredParticipantsHaveLowestSortKeys(participants);

            bool NewParticipantsCannotHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
                => invitationValidator.NewParticipantsCannotHaveLowestSortKeys(participants); 

            bool ParticipantListMustBeValid(IList<ParticipantsForCommand> participants)
                => invitationValidator.IsValidParticipantList(participants);

            bool ParticipantMustHaveId(ParticipantsForCommand participant)
                => invitationValidator.ParticipantMustHaveId(participant);
        }
    }
}
