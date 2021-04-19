using System.Threading;
using System.Threading.Tasks;
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
            if (notification.Emails.Count == 0)
            {
                return Task.CompletedTask;
            }

            var baseUrl = _meetingOptions.CurrentValue.PcsBaseUrl;
            var id = notification.Id;
            var title = notification.Title;
            var plantId = notification.Plant.Split('$')[1];

            var subject = $"Completed notification: IPO-{id}";
            var body =
                $"<p>IPO-{id}: {title} has been completed and is ready for your attention to sign and accept.</p>" +
                "<p>Click the link to review " +
                $"<a href=\"{baseUrl}{plantId}/InvitationForPunchOut/{id}\">IPO-{id}</a>.</p>";

            return _emailService.SendEmailsAsync(notification.Emails, subject, body, cancellationToken);
        }
    }
}
