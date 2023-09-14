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
        protected readonly IOptionsMonitor<SmtpOptions> _smtp;

        public SmtpService(IOptionsMonitor<SmtpOptions> smtp)
        {
            _smtp = smtp;
        }

        public void SendAsync(MailMessage message, string token)
        {
            // SendAsync is not thread-safe, so to ensure it is not busy sending an mail I instantiate an new SmtpClient
            var client = new System.Net.Mail.SmtpClient(_smtp.CurrentValue.Server, _smtp.CurrentValue.Port);
            client.EnableSsl = _smtp.CurrentValue.EnableSSL;
            client.Credentials = new NetworkCredential(_smtp.CurrentValue.Email, _smtp.CurrentValue.Password);
            client.SendAsync(message, token);
        }

        private String CreateInviteString(MailMessage message, Invitation invitation)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("BEGIN:VCALENDAR");
            str.AppendLine("PRODID:-//Equinor//ProCoSys//EN");
            str.AppendLine("VERSION:2.0");
            str.AppendLine("METHOD:REQUEST");
            str.AppendLine("X-MS-OLK-FORCEINSPECTOROPEN:TRUE");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", invitation.StartTimeUtc));
            str.AppendLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
            str.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmssZ}", invitation.EndTimeUtc));
            str.AppendLine("LOCATION: " + invitation.Location);
            str.AppendLine(string.Format("UID:{0}", Guid.NewGuid()));
            str.AppendLine(string.Format("DESCRIPTION:{0}", message.Body));
            str.AppendLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", message.Body));
            str.AppendLine(string.Format("SUMMARY:{0}", message.Subject));
            str.AppendLine(string.Format("ORGANIZER;CN=\"{0}\":mailto:{1}", message.From.DisplayName, message.From.Address));
            message.To.ToList().ForEach(y =>
            {
                str.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", y.DisplayName, y.Address));
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
            byte[] byteArray = Encoding.ASCII.GetBytes(inviteString);
            MemoryStream stream = new MemoryStream(byteArray);

            Attachment attachment = new Attachment(stream, "invite.ics");
            attachment.ContentType = new ContentType("text/calendar");

            attachment.TransferEncoding = TransferEncoding.QuotedPrintable;
            return attachment;
        }

        public async Task SendSmtpWithInviteAsync(Invitation invitation, string projectName, Person organizer, string pcsBaseUrl, CreateInvitationCommand request)
        {
            var baseUrl = InvitationHelper.GetBaseUrl(pcsBaseUrl, invitation.Plant);

            var message = new MailMessage()
            {
                From = new MailAddress(organizer.Email, $"{organizer.FirstName} {organizer.LastName}"),
                Subject = InvitationHelper.GenerateMeetingTitle(invitation, projectName, request.Type,
                            request.Type == DisciplineType.DP ? request.McPkgScope : request.CommPkgScope),
                Body = InvitationHelper.GenerateMeetingDescription(invitation, baseUrl, organizer, projectName, _smtp.CurrentValue.FakeEmail),
                Priority = MailPriority.Normal
            };
            invitation.Participants.ToList().ForEach(y =>
            {
                message.To.Add(new MailAddress(y.Email, $"{y.FirstName} {y.LastName}"));
            });

            AlternateView avBody = AlternateView.CreateAlternateViewFromString(message.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
            message.AlternateViews.Add(avBody);

            String inviteString = CreateInviteString(message, invitation);
            Attachment attachment = CreateInviteAttachment(inviteString);
            message.Attachments.Add(attachment);

            ContentType contype = new ContentType("text/calendar");
            contype.CharSet = "UTF-8";
            contype.Parameters.Add("method", "REQUEST");
            contype.Parameters.Add("name", "invite.ics");

            AlternateView avCal = AlternateView.CreateAlternateViewFromString(inviteString, contype);
            avCal.TransferEncoding = TransferEncoding.QuotedPrintable;
            message.AlternateViews.Add(avCal);
            SendAsync(message, ToString());
        }
    }
}
