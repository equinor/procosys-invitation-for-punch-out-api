using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.IPO.Email;
using MediatR;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoCompletedEmailHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;

        public IpoCompletedEmailHandler(IEmailService emailService, IOptionsMonitor<MeetingOptions> meetingOptions)
        {
            _emailService = emailService;
            _meetingOptions = meetingOptions;
        } 

        public Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            var constructionCompanyParticipant = notification.Invitation.Participants.SingleOrDefault(p =>
                p.SortKey == 1 && p.Organization == Organization.ConstructionCompany);

            if (constructionCompanyParticipant?.Email == null)
            {
                return Task.CompletedTask;
            }

            var baseUrl = _meetingOptions.CurrentValue.PcsBaseUrl;
            var id = notification.Invitation.Id;
            var title = notification.Invitation.Title;

            var subject = $"Completed notification: IPO-{id}";
            var body =
                $"<p>IPO-{id}: {title} has been completed and is ready for your attention.</p>" +
                "<p>Click the link to review " +
                $"<a href=\"{baseUrl}/{notification.Plant}/InvitationForPunchOut/{id}\">IPO-{id}</a>.</p>";

            return _emailService.SendEmailAsync(constructionCompanyParticipant.Email, subject, body, cancellationToken);
        }
    }
}
