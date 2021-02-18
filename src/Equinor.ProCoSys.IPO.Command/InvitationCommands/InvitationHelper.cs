using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class InvitationHelper
    {
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

        private static ParticipantIdentifier CreateParticipantIdentifier(ProCoSysPerson person) 
            => IsValidEmail(person.Email) ? 
                new ParticipantIdentifier(person.Email) : 
                new ParticipantIdentifier(new Guid(person.AzureOid));

        private static ParticipantType GetParticipantType(bool required) 
            => required ? ParticipantType.Required : ParticipantType.Optional;

        private static bool IsValidEmail(string email)
            => new EmailAddressAttribute().IsValid(email);

        public static string GenerateMeetingTitle(Invitation invitation) 
            => $"Invitation for punch-out, IPO-{invitation.Id}, Project: {invitation.ProjectName}";

        public static string GenerateMeetingDescription(Invitation invitation, string baseUrl)
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
                meetingDescription += GenerateMcPkgTable(invitation, baseUrl);
            }

            if (invitation.CommPkgs.Count > 0)
            {
                meetingDescription += GenerateCommPkgTable(invitation, baseUrl);
            }

            meetingDescription += $"</br><a href='{baseUrl}" + $"/InvitationForPunchOut/{invitation.Id}'>" + "Open invitation for punch-out in ProCoSys.</a>";

            return meetingDescription;
        }

        private static string GenerateMcPkgTable(Invitation invitation, string baseUrl)
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
                    $"<td style='border: 1px solid black;'><a href='{baseUrl}/Completion#McPkg|?projectName={invitation.ProjectName}&mcpkgno={mcPkg.McPkgNo}'>{mcPkg.McPkgNo}</a></td>" +
                    $"<td style='border: 1px solid black;'>{mcPkg.Description}</td>" +
                    $"<td style='border: 1px solid black;'><a href='{baseUrl}/Completion#CommPkg|?projectName={invitation.ProjectName}&commpkgno={mcPkg.CommPkgNo}'>{mcPkg.CommPkgNo}</a></td>" +
                    "</tr>";
            }
            table += $"</table>";
            return table;
        }

        private static string GenerateCommPkgTable(Invitation invitation, string baseUrl)
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
                    $"<td style='border: 1px solid black;'><a href='{baseUrl}/Completion#CommPkg|?projectName={invitation.ProjectName}&commpkgno={commPkg.CommPkgNo}'>{commPkg.CommPkgNo}</a></td>" +
                    $"<td style='border: 1px solid black;'>{commPkg.Description}</td>" +
                    "</tr>";
            }

            table += $"</table>";
            return table;
        }
    }
}
