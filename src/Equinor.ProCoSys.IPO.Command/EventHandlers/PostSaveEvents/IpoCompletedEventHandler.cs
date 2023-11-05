using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.Events.PostSave;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents
{
    public class IpoCompletedEventHandler : INotificationHandler<IpoCompletedEvent>
    {
        private readonly ILogger<IpoCompletedEventHandler> _logger;
        private readonly IPcsBusSender _pcsBusSender;
        private readonly IEmailService _emailService;
        private readonly IOptionsMonitor<MeetingOptions> _meetingOptions;

        public IpoCompletedEventHandler(
            IPcsBusSender pcsBusSender, 
            IEmailService emailService, 
            IOptionsMonitor<MeetingOptions> meetingOptions,
            ILogger<IpoCompletedEventHandler> logger)
        {
            _pcsBusSender = pcsBusSender;
            _emailService = emailService;
            _meetingOptions = meetingOptions;
            _logger = logger;
        } 

        public async Task Handle(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            await SendBusTopicAsync(notification);
            try
            {
                await SendEmailAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An email for the punch out {notification.Title} could not be sent.");
                throw new IpoSendMailException("It is currently not possible to send email about the completed punch-out since there is a problem when sending email to recipients.", ex);
            }
        }

        private async Task SendBusTopicAsync(IpoCompletedEvent notification)
        {
            var eventMessage = new BusEventMessage
            {
                Plant = notification.Plant, Event = "Completed", InvitationGuid = notification.SourceGuid
            };

            await _pcsBusSender.SendAsync(IpoTopic.TopicName, JsonSerializer.Serialize(eventMessage));
        }

        private async Task SendEmailAsync(IpoCompletedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Emails.Count == 0)
            {
                return;
            }

            var baseUrl = _meetingOptions.CurrentValue.PcsBaseUrl;
            var id = notification.Id;
            var title = notification.Title;
            var plantId = notification.Plant.Split('$').Last();

            var subject = $"Completed notification: IPO-{id}";
            var body =
                $"<p>IPO-{id}: {title} has been completed and is ready for your attention to sign and accept.</p>" +
                "<p>Click the link to review " +
                $"<a href=\"{baseUrl}{plantId}/InvitationForPunchOut/{id}\">IPO-{id}</a>.</p>";

            await _emailService.SendEmailsAsync(notification.Emails, subject, body, cancellationToken);
        }
    }
}
