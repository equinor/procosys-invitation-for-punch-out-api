using FluentValidation;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditInvitationDtoValidator : AbstractValidator<EditInvitationDto>
    {
        public EditInvitationDtoValidator() => RuleFor(x => x.Meeting).NotNull().SetValidator(new EditMeetingDtoValidator());
    }

    public class EditMeetingDtoValidator : AbstractValidator<EditMeetingDto>
    {
        public EditMeetingDtoValidator()
        {
            RuleFor(x => x.BodyHtml).MaximumLength(8192);
            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
            RuleFor(x => x.Location).MaximumLength(1024);
            RuleFor(x => x.Title).MaximumLength(1024);
        }
    }
}
