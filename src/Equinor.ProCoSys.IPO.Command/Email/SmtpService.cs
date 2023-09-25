using System.Net.Mail;
using System.Net;
using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using System.IO;
using System.Net.Mime;
using System.Text;
using Attachment = System.Net.Mail.Attachment;
using System.Linq;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Options;
using Equinor.ProCoSys.Auth.Client;

namespace Equinor.ProCoSys.IPO.Command.Email
{
    public class SmtpService : ISmtpService
    {
        protected readonly IOptionsMonitor<SmtpOptions> _smtpOptions;

        public SmtpService(IOptionsMonitor<SmtpOptions> smtpOptions) => _smtpOptions = smtpOptions;

        public async Task SendAsync(MailMessage message)
        {
            // SendMailAsync is not thread-safe, so to ensure it is not busy sending an mail I instantiate an new SmtpClient
            var client = new SmtpClient(_smtpOptions.CurrentValue.Server, _smtpOptions.CurrentValue.Port);
            client.EnableSsl = _smtpOptions.CurrentValue.EnableSSL;
            client.Credentials = new NetworkCredential(_smtpOptions.CurrentValue.From, _smtpOptions.CurrentValue.Password);
            await client.SendMailAsync(message);
        }

        public async Task SendSmtpWithInviteAsync(Invitation invitation, string projectName, Person organizer, string pcsBaseUrl, CreateInvitationCommand request)
        {
            var baseUrl = InvitationHelper.GetBaseUrl(pcsBaseUrl, invitation.Plant);

            var message = new MailMessage()
            {
                From = new MailAddress(organizer.Email, $"{organizer.FirstName} {organizer.LastName}"),
                Subject = InvitationHelper.GenerateMeetingTitle(invitation, projectName, request.Type,
                            request.Type == DisciplineType.DP ? request.McPkgScope : request.CommPkgScope),
                Body = InvitationHelper.GenerateMeetingDescription(invitation, baseUrl, organizer, projectName, true, _smtpOptions.CurrentValue.FakeEmail),
                Priority = MailPriority.Normal
            };
            invitation.Participants.ToList().ForEach(y =>
            {
                message.To.Add(new MailAddress(y.Email, $"{y.FirstName} {y.LastName}"));
            });

            var avBody = AlternateView.CreateAlternateViewFromString(message.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
            message.AlternateViews.Add(avBody);

            var inviteString = CreateInviteString(message, invitation);
            var attachment = CreateInviteAttachment(inviteString);
            message.Attachments.Add(attachment);

            var contype = new ContentType("text/calendar");
            contype.CharSet = "UTF-8";
            contype.Parameters.Add("method", "REQUEST");
            contype.Parameters.Add("name", "invite.ics");

            var avCal = AlternateView.CreateAlternateViewFromString(inviteString, contype);
            avCal.TransferEncoding = TransferEncoding.QuotedPrintable;
            message.AlternateViews.Add(avCal);
            await SendAsync(message);
        }

        private String CreateInviteString(MailMessage message, Invitation invitation)
        {
            var str = new StringBuilder();
            str.AppendLine("BEGIN:VCALENDAR");
            str.AppendLine("PRODID:-//Equinor//ProCoSys//EN");
            str.AppendLine("VERSION:2.0");
            str.AppendLine("METHOD:REQUEST");
            str.AppendLine("X-MS-OLK-FORCEINSPECTOROPEN:TRUE");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine($"DTSTART:{invitation.StartTimeUtc:yyyyMMddTHHmmssZ}");
            str.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            str.AppendLine($"DTEND:{invitation.EndTimeUtc:yyyyMMddTHHmmssZ}");
            str.AppendLine($"LOCATION: {invitation.Location}");
            str.AppendLine($"UID:{Guid.NewGuid()}");
            str.AppendLine($"DESCRIPTION:{message.Body}");
            str.AppendLine($"X-ALT-DESC;FMTTYPE=text/html:{message.Body}");
            str.AppendLine($"SUMMARY:{message.Subject}");
            str.AppendLine($"ORGANIZER;CN=\"{message.From.DisplayName}\":mailto:{message.From.Address}");
            message.To.ToList().ForEach(y =>
            {
                str.AppendLine($"ATTENDEE;CN=\"{y.DisplayName}\";RSVP=TRUE:mailto:{y.Address}");
            });
            str.AppendLine("CLASS:PUBLIC");
            str.AppendLine("TRANSP:OPAQUE");

            str.AppendLine("BEGIN:VALARM");
            str.AppendLine("TRIGGER:-PT15M");
            str.AppendLine("ACTION:DISPLAY");
            str.AppendLine("DESCRIPTION:Reminder");
            str.AppendLine("END:VALARM");
            str.AppendLine("END:VEVENT");
            str.AppendLine("END:VCALENDAR");
            return str.ToString();
        }

        private Attachment CreateInviteAttachment(string inviteString)
        {
            var byteArray = Encoding.ASCII.GetBytes(inviteString);
            var stream = new MemoryStream(byteArray);

            var attachment = new Attachment(stream, "invite.ics");
            attachment.ContentType = new ContentType("text/calendar");

            attachment.TransferEncoding = TransferEncoding.QuotedPrintable;
            return attachment;
        }
    }
}
