using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.IPO.Email;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly IPcsBusSender _pcsBusSender;
        private readonly IEmailService _emailService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;

        public IpoCompletedEventHandler(IPcsBusSender pcsBusSender, IEmailService emailService, IOptionsMonitor<MeetingOptions> meetingOptions)
        {
            _pcsBusSender = pcsBusSender;
            _emailService = emailService;
            _meetingOptions = meetingOptions;
        } 

        public async Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            await SendBusTopic(notification, cancellationToken);
            await SendEmail(notification, cancellationToken);
        }

        private async Task SendBusTopic(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = notification.Plant, Event = "Completed", InvitationGuid = notification.ObjectGuid
            };

            await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }

        private async Task SendEmail(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Emails.Count == 0)
            {
                return;
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

            await _emailService.SendEmailsAsync(notification.Emails, subject, body, cancellationToken);
        }
    }
}
