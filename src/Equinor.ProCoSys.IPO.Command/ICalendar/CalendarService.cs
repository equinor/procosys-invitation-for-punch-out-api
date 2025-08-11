using System;
using System.Linq;
using System.Text;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.Graph.Models;
using Invitation = Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation;
using Person = Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person;

namespace Equinor.ProCoSys.IPO.Command.ICalendar
{
    public class CalendarService : ICalendarService
    {
        public Message CreateMessage(Invitation invitation, string projectName, Person organizer, string pcsBaseUrl, CreateInvitationCommand request)
        {
            var baseUrl = InvitationHelper.GetBaseUrl(pcsBaseUrl, invitation.Plant);
            var subject = InvitationHelper.GenerateMeetingTitle(invitation, projectName, request.Type,
                            request.Type == DisciplineType.DP ? request.McPkgScope : request.CommPkgScope);
            var content = InvitationHelper.GenerateMeetingDescription(invitation, baseUrl, organizer, projectName, true);

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = InvitationHelper.GenerateMeetingFallbackDescription()
                },
                ToRecipients =
                [
                    new()
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = organizer.Email, Name = $"{organizer.FirstName} {organizer.LastName}"
                        }
                    }
                ]
            };

            var invitationString = CreateInvitation(organizer, subject, content, invitation);
            var attachment = CreateICalendarInvitationAttachment(invitationString);
            message.Attachments = [attachment];
            return message;
        }

        private static string CreateInvitation(Person organizer, string subject, string content, Invitation invitation)
        {
            var str = new StringBuilder();
            str.AppendLine("BEGIN:VCALENDAR");
            str.AppendLine("PRODID:-//Equinor//ProCoSys//EN");
            str.AppendLine("VERSION:2.0");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine($"DTSTART:{invitation.StartTimeUtc:yyyyMMddTHHmmssZ}");
            str.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            str.AppendLine($"DTEND:{invitation.EndTimeUtc:yyyyMMddTHHmmssZ}");
            str.AppendLine($"LOCATION: {invitation.Location.StripHtml()}");
            str.AppendLine($"UID:{Guid.NewGuid()}");
            str.AppendLine($"DESCRIPTION:{content.StripHtml()}");
            str.AppendLine($"X-ALT-DESC;FMTTYPE=text/html:{content}");
            str.AppendLine($"SUMMARY:{subject.StripHtml()}");
            str.AppendLine("STATUS:CONFIRMED");
            str.AppendLine($"ORGANIZER;CN=\"{organizer.FirstName} {organizer.LastName}\":mailto:{organizer.Email}");

            invitation.Participants.ToList().ForEach(y =>
            {
                var recipientDisplayName = $"{y.FirstName} {y.LastName}";
                if (!string.IsNullOrWhiteSpace(recipientDisplayName))
                {
                    str.AppendLine($"ATTENDEE;CN=\"{recipientDisplayName}\";PARTSTAT=NEEDS-ACTION;RSVP=TRUE:mailto:{y.Email}");
                }
                else
                {
                    str.AppendLine($"ATTENDEE;PARTSTAT=NEEDS-ACTION;RSVP=TRUE:mailto:{y.Email}");
                }
            });
            str.AppendLine("BEGIN:VALARM");
            str.AppendLine("TRIGGER:-PT15M");
            str.AppendLine("ACTION:DISPLAY");
            str.AppendLine("DESCRIPTION:Reminder");
            str.AppendLine("END:VALARM");
            str.AppendLine("END:VEVENT");
            str.AppendLine("END:VCALENDAR");
            return str.ToString();
        }

        private static FileAttachment CreateICalendarInvitationAttachment(string inviteString)
        {
            var byteArray = Encoding.UTF8.GetBytes(inviteString);
            return new FileAttachment()
            {

                OdataType = "#microsoft.graph.fileAttachment",
                ContentBytes = byteArray,
                ContentType = "text/calendar",
                Name = "invitation.ics"
            };
        }

    }
}
