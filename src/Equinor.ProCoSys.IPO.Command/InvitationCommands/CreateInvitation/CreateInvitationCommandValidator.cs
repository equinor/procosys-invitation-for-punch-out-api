using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
    {
        public CreateInvitationCommandValidator(IInvitationValidator invitationValidator)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                //input validators
                .Must(command => command.Participants != null && command.Participants.Any())
                .WithMessage("Participants must be invited!")
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
                .WithMessage("Contractor must be first and Construction Company must be second!")
                .Must(command => ParticipantListMustBeValid(command.Participants))
                .WithMessage("Each participant must contain an oid!")
                .Must(command => ParticipantListMustHaveValidEmails(command.Participants))
                .WithMessage("One, or more, of the email-addresses for the participants is invalid.");

            RuleForEach(command => command.Participants)
                .Must(participant => participant.SortKey >= 0)
                .WithMessage((_, participant) =>
                    $"Sort key for participant must be a non negative number! SortKey={participant.SortKey}")
                .Must(FunctionalRoleParticipantsMustBeValid)
                .WithMessage((_, participant) =>
                    $"Functional role code must be between 3 and {Participant.FunctionalRoleCodeMaxLength} characters! Code={participant.InvitedFunctionalRole.Code}");

            bool MustHaveValidScope(DisciplineType type, IList<string> mcPkgScope, IList<string> commPkgScope)
                => invitationValidator.IsValidScope(type, mcPkgScope, commPkgScope);

            bool TwoFirstParticipantsMustBeSetWithCorrectOrganization(IList<ParticipantsForCommand> participants)
                => invitationValidator.RequiredParticipantsMustBeInvited(participants);

            bool RequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
                => invitationValidator.OnlyRequiredParticipantsHaveLowestSortKeys(participants); 

            bool ParticipantListMustBeValid(IList<ParticipantsForCommand> participants)
                => invitationValidator.IsValidParticipantList(participants);

            bool ParticipantListMustHaveValidEmails(IList<ParticipantsForCommand> participants)
            {
                foreach (var participant in participants)
                {
                    if (!string.IsNullOrEmpty(participant.InvitedExternalEmail?.Email))
                    {
                        var IsValid = EmailValidator.IsValid(participant.InvitedExternalEmail?.Email);
                        if (!IsValid)
                        {
                            return false;
                        }
                    }
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
