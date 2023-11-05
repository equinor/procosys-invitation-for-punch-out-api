using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Fusion.Integration.Meeting;
using System.Linq;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class InvitationHelper
    {
        public static string GetBaseUrl(string pcsBaseUrl, string plant) 
            => $"{pcsBaseUrl.Trim('/')}/{plant.Substring(4, plant.Length - 4).ToUpper()}";

        public static bool ParticipantIsSigningParticipant(ParticipantsForCommand participant) 
            => participant.Organization != Organization.External && participant.Organization != Organization.Supplier;

        public static async Task<bool> HasIpoAdminPrivilegeAsync(
            IPermissionCache permissionCache,
            IPlantProvider plantProvider,
            ICurrentUserProvider currentUserProvider)
        {
            var permissions = await permissionCache.GetPermissionsForUserAsync(plantProvider.Plant, currentUserProvider.GetCurrentUserOid());
            return permissions != null && permissions.Contains("IPO/ADMIN");
        }

        public static List<BuilderParticipant> SplitAndCreateOutlookParticipantsFromEmailList(
            string emails)
        {
            var participants = new List<BuilderParticipant>();
            var splitEmails = emails.Split(";");
            foreach (var email in splitEmails)
            {
                participants.Add(new BuilderParticipant(ParticipantType.Required,
                    new ParticipantIdentifier(email)));
            }
            return participants;
        }

        public static List<BuilderParticipant> AddPersonToOutlookParticipantList(
            ProCoSysPerson person,
            List<BuilderParticipant> participants,
            bool required = true)
        {
            var participantType = GetParticipantType(required);
            var participantIdentifier = CreateParticipantIdentifier(person);

            participants.Add(new BuilderParticipant(participantType, participantIdentifier));
            return participants;
        }

        public static string GenerateMeetingTitle(Invitation invitation, string projectName, DisciplineType type, IList<string> scope)
        {
            var ipoPart = $"IPO-{invitation.Id}";
            var projectPart = $"Project: {projectName}";
            var scopePart = $"{type}: {string.Join(",", scope)}";
            return $"{ipoPart}. {projectPart}. {scopePart}";
        }

        public static string GenerateMeetingDescription(Invitation invitation, string baseUrl, Person organizer, string projectName, bool isFallback)
        {
            var meetingDescription = "<h4>You have been invited to attend a punch round.</h4>";
            meetingDescription += $"<p>Title: {invitation.Title}</p>";
            meetingDescription += $"<p>Type: {invitation.Type}</p>";
            var location = string.IsNullOrWhiteSpace(invitation.Location) ? "-" : invitation.Location;
            meetingDescription += $"<p>Location: {location}</p>";
            var description = string.IsNullOrWhiteSpace(invitation.Description) ? "-" : invitation.Description;
            meetingDescription += $"<p>Description: {description}</p>";
            meetingDescription += $"<p>Scope: </p>";

            if (invitation.McPkgs.Count > 0)
            {
                meetingDescription += GenerateMcPkgTable(invitation, baseUrl, projectName);
            }

            if (invitation.CommPkgs.Count > 0)
            {
                meetingDescription += GenerateCommPkgTable(invitation, baseUrl, projectName);
            }

            meetingDescription += $"<br/><a href='{baseUrl}" + $"/InvitationForPunchOut/{invitation.Id}'>" + "Open invitation for punch-out in ProCoSys.</a><br/><br/>";
            meetingDescription += $"Best regards,<br/>";
            meetingDescription += $"{organizer.FirstName} {organizer.LastName}<br/><br/>";

            if (isFallback)
            {
                meetingDescription += $@"<p>#########################################################################################################<br/>
                    NOTE: Due to technical issues with the regular system used to send invitation for punch-out,<br/>
                    a fallback solution was used to send this invite.<br/>
                    This has the concequence that the related meeting does not have the same level of integration and features as you are used to.<br/>                    
                    #########################################################################################################<br/>";
            }

            return meetingDescription;
        }

        public static string GenerateMeetingFallbackDescription()
        {
            var meetingDescription = "<h4>Invitation to a punch round - fallback solution</h4>";
            meetingDescription += $@"<p>Due to technical issues with the regular system used to send invitation for punch-out, a fallback solution is used.<br/>
                    This has the concequence that the invitation has been made available to you through an attached iCalendar.<br/>
                    To ensure that the invitation is made available to the participants follow these steps:<br/><br/>
                    - Left click the file drop down<br/>
                    - Select open<br/>
                    - Add the attached iCalendar to Outlook by clicking yes.<br/>
                    - Select 'send update'.<br/>
                    - Select 'send update to all attendees'<br/>";

            return meetingDescription;
        }

        private static string GenerateMcPkgTable(Invitation invitation, string baseUrl, string projectName)
        {
            var table = "<table style='border-collapse:collapse;'>" +
                                  "<tr>" +
                                  "<td style='border: 1px solid black;padding-right:5px;'>Mc pkg no</td>" +
                                  "<td style='border: 1px solid black;padding-right:5px;'>Description</td>" +
                                  "<td style='border: 1px solid black;padding-right:5px;'>Comm pkg no</td>" +
                                  "</tr>";

            foreach (var mcPkg in invitation.McPkgs)
            {
                table +=
                    "<tr>" +
                    $"<td style='border: 1px solid black;'><a href='{baseUrl}/Completion#McPkg|?projectName={projectName}&mcpkgno={mcPkg.McPkgNo}'>{mcPkg.McPkgNo}</a></td>" +
                    $"<td style='border: 1px solid black;'>{mcPkg.Description}</td>" +
                    $"<td style='border: 1px solid black;'><a href='{baseUrl}/Completion#CommPkg|?projectName={projectName}&commpkgno={mcPkg.CommPkgNo}'>{mcPkg.CommPkgNo}</a></td>" +
                    "</tr>";
            }
            table += $"</table>";
            return table;
        }

        private static string GenerateCommPkgTable(Invitation invitation, string baseUrl, string projectName)
        {
            var table = "<table style='border-collapse:collapse;'>" +
                                  "<tr style='font-weight:bold;'>" +
                                  "<td style='border: 1px solid black;padding-right:5px;'>Comm pkg no</td>" +
                                  "<td style='border: 1px solid black;padding-right:5px;'>Description</td>" +
                                  "</tr>";

            foreach (var commPkg in invitation.CommPkgs)
            {
                table +=
                    "<tr>" +
                    $"<td style='border: 1px solid black;'><a href='{baseUrl}/Completion#CommPkg|?projectName={projectName}&commpkgno={commPkg.CommPkgNo}'>{commPkg.CommPkgNo}</a></td>" +
                    $"<td style='border: 1px solid black;'>{commPkg.Description}</td>" +
                    "</tr>";
            }

            table += $"</table>";
            return table;
        }
        private static ParticipantIdentifier CreateParticipantIdentifier(ProCoSysPerson person)
            => IsValidEmail(person.Email) ?
                new ParticipantIdentifier(person.Email) :
                new ParticipantIdentifier(new Guid(person.AzureOid));

        private static ParticipantType GetParticipantType(bool required)
            => required ? ParticipantType.Required : ParticipantType.Optional;

        private static bool IsValidEmail(string email)
            => new EmailAddressAttribute().IsValid(email);
    }
}
