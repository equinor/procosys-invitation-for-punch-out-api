using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate
{
    public static class EventTypeExtensions
    {
        public static string GetDescription(this EventType eventType, Participant participant)
        {
            var user = $"{participant.FirstName} {participant.LastName}";
            var functionalRole = participant.FunctionalRoleCode;
            var organization = participant.Organization.GetDescription();

            if (string.IsNullOrEmpty(functionalRole))
            {
                return $"{eventType.GetDescription()} by {user} for {organization}";
            }
            else
            {
                return $"{eventType.GetDescription()} by {user} on behalf of {functionalRole} for {organization}";
            }
        }

        public static string GetDescription(this EventType eventType, Participant participant, Person person)
        {
            var user = $"{person.FirstName} {person.LastName}";
            var functionRole = participant.FunctionalRoleCode;
            var organization = participant.Organization.GetDescription();

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
