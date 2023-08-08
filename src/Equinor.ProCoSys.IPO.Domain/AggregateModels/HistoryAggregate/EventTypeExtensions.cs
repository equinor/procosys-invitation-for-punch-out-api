using System.ComponentModel;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate
{
    public static class EventTypeExtensions
    {
        public static string GetDescription(this EventType eventType, Participant participant)
        {
            string user = $"{participant.FirstName} {participant.LastName}";
            string functionRole = participant.FunctionalRoleCode;
            string organization = participant.Organization.GetDescription();

            if (string.IsNullOrEmpty(functionRole))
            {
                return $"{eventType.GetDescription()} by {user} for {organization}";
            }
            else
            {
                return $"{eventType.GetDescription()} by {user} on behalf of {functionRole} for {organization}";
            }
        }

        public static string GetDescription(this EventType eventType, Participant participant, Person person)
        {
            string user = $"{person.FirstName} {person.LastName}";
            string functionRole = participant.FunctionalRoleCode;
            string organization = participant.Organization.GetDescription();

            if (string.IsNullOrEmpty(functionRole))
            {
                return $"{eventType.GetDescription()} by {user} for {organization}";
            }
            else
            {
                return $"{eventType.GetDescription()} by {user} on behalf of {functionRole} for {organization}";
            }
        }
    }
}
