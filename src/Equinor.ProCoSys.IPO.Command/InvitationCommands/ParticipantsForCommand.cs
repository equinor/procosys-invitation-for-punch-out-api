using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands
{
    public class ParticipantsForCommand
    {
        public ParticipantsForCommand(
            Organization organization,
            IExternalEmailForCommand externalEmail,
            IPersonForCommand person,
            IFunctionalRoleForCommand functionalRole,
            int sortKey)
        {
            Organization = organization;
            FunctionalRole = functionalRole;
            Person = person;
            ExternalEmail = externalEmail;
            SortKey = sortKey;
        }
        public Organization Organization { get; }
        public int SortKey { get; }
        public IExternalEmailForCommand ExternalEmail { get; }
        public IPersonForCommand Person { get; }
        public IFunctionalRoleForCommand FunctionalRole { get; }
    }
}
