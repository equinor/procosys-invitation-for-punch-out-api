using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
    {
        public CreateInvitationCommandValidator(IInvitationValidator invitationValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(command => command)
                .Must((command) => command.ProjectName.Length > 2)
                .WithMessage(command =>
                    $"Project name must be at least 3 characters! ProjectName={command.ProjectName}")
                .Must((command) => command.Description == null || command.Description.Length < 4000)
                .WithMessage(command =>
                    $"Description cannot be more than 4000 characters! Description={command.Description}")
                .Must((command) => command.StartTime < command.EndTime)
                .WithMessage(command =>
                    $"Start time must be before end time! Start={command.StartTime} End={command.EndTime}")
                .Must((command) => command.Title.Length > 2 && command.Title.Length < Invitation.TitleMaxLength)
                .WithMessage(command =>
                    $"Title must be between 3 and 1024 characters! Title={command.Title}")
                .Must((command) => command.Description == null || command.Location.Length < 1024)
                .WithMessage(command =>
                    $"Location cannot be more than 1024 characters! Location={command.Location}");

            RuleFor(command => command)
                .MustAsync((command, token) => TitleMustBeUniqueOnProject(command.ProjectName, command.Title, token))
                .WithMessage(command =>
                    $"IPO with this title already exists in project! Title={command.Title}")
                .Must((command) => MustHaveValidScope(command.McPkgScope, command.CommPkgScope))
                .WithMessage(command =>
                    $"Not a valid scope! Choose either mc scope or comm pkg scope")
                .Must((command) => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.Participants))
                .WithMessage(command =>
                    $"Contractor and Construction Company must be invited!")
                .Must((command) => RequiredParticipantsHaveLowestSortKeys(command.Participants))
                .WithMessage(command =>
                    $"SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company!")
                .Must((command) => ParticipantListMustBeValid(command.Participants))
                .WithMessage(command =>
                    $"Each participant must contain an email or oid!");

            async Task<bool> TitleMustBeUniqueOnProject(string projectName, string title, CancellationToken token)
                => !await invitationValidator.IpoTitleExistsInProjectAsync(projectName, title, token);

            bool MustHaveValidScope(IList<McPkgScopeForCommand> mcPkgScope, IList<CommPkgScopeForCommand> commPkgScope)
                => invitationValidator.IsValidScope(mcPkgScope, commPkgScope);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants);

            bool RequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
                => invitationValidator.OnlyRequiredParticipantsHaveLowestSortKeys(participants); 

            bool ParticipantListMustBeValid(IList<ParticipantsForCommand> participants)
                => invitationValidator.IsValidParticipantList(participants);
        }
    }
}
