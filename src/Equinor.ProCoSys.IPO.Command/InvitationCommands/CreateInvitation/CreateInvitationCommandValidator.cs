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
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                //input validators
                .Must(command => command.Participants != null)
                .WithMessage(command =>
                    "Participants cannot be null!")
                .Must(command =>
                    command.ProjectName != null && 
                    command.ProjectName.Length >= Invitation.ProjectNameMinLength &&
                    command.ProjectName.Length < Invitation.ProjectNameMaxLength)
                .WithMessage(command =>
                    $"Project name must be between {Invitation.ProjectNameMinLength} and {Invitation.ProjectNameMaxLength} characters! ProjectName={command.ProjectName}")
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
                .MustAsync((command, token) => TitleMustBeUniqueOnProject(command.ProjectName, command.Title, token))
                .WithMessage(command =>
                    $"IPO with this title already exists in project! Title={command.Title}")
                .Must(command => MustHaveValidScope(command.McPkgScope, command.CommPkgScope))
                .WithMessage(command =>
                    "Not a valid scope! Choose either mc scope or comm pkg scope")
                .Must(command => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.Participants))
                .WithMessage(command =>
                    "Contractor and Construction Company must be invited!")
                .Must(command => RequiredParticipantsHaveLowestSortKeys(command.Participants))
                .WithMessage(command =>
                    "SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company!")
                .Must(command => ParticipantListMustBeValid(command.Participants))
                .WithMessage(command =>
                    "Each participant must contain an email or oid!");

            async Task<bool> TitleMustBeUniqueOnProject(string projectName, string title, CancellationToken token)
                => !await invitationValidator.IpoTitleExistsInProjectAsync(projectName, title, token);

            bool MustHaveValidScope(IList<string> mcPkgScope, IList<string> commPkgScope)
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
