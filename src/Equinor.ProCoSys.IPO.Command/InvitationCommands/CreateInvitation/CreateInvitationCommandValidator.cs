using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
    {
        public CreateInvitationCommandValidator()
        {
            RuleForEach(x => x.Meeting.RequiredParticipantEmails).EmailAddress();
            RuleForEach(x => x.Meeting.OptionalParticipantEmails).EmailAddress();
        }
    }
}
