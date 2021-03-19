using System.Collections.Generic;
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
                .Must(command => MustHaveValidScope(command.Type, command.McPkgScope, command.CommPkgScope))
                .WithMessage("Not a valid scope! Choose either DP with mc scope or MDP with comm pkg scope")
                .Must(command => TwoFirstParticipantsMustBeSetWithCorrectOrganization(command.Participants))
                .WithMessage("Contractor and Construction Company must be invited!")
                .Must(command => RequiredParticipantsHaveLowestSortKeys(command.Participants))
                .WithMessage("SortKey 0 is reserved for Contractor, and SortKey 1 is reserved for Construction Company!")
                .Must(command => ParticipantListMustBeValid(command.Participants))
                .WithMessage("Each participant must contain an email or oid!");

            bool MustHaveValidScope(DisciplineType type, IList<string> mcPkgScope, IList<string> commPkgScope)
                => invitationValidator.IsValidScope(type, mcPkgScope, commPkgScope);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants);

            bool RequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
                => invitationValidator.OnlyRequiredParticipantsHaveLowestSortKeys(participants); 

            bool ParticipantListMustBeValid(IList<ParticipantsForCommand> participants)
                => invitationValidator.IsValidParticipantList(participants);
        }
    }
}
