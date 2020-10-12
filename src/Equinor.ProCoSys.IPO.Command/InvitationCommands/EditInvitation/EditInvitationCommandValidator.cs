using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation
{
    public class EditInvitationCommandValidator : AbstractValidator<EditInvitationCommand>
    {
        public EditInvitationCommandValidator()
        {
            RuleForEach(x => x.Meeting.RequiredParticipantEmails).EmailAddress();
            RuleForEach(x => x.Meeting.OptionalParticipantEmails).EmailAddress();
        }
    }
}
