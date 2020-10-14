using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation
{
    public class ParticipantsForCommand : IRequest<Result<Unit>>
    {
        public ParticipantsForCommand(
            Organization organization,
            string externalEmail,
            PersonForCommand person,
            FunctionalRoleForCommand functionalRole,
            int sortKey)
        {
            Organization = organization;
            FunctionalRole = functionalRole;
            Person = person;
            ExternalEmail = externalEmail;
            SortKey = sortKey;
        }
        public Organization Organization { get; set; }
        public int SortKey { get; set; }
        public string ExternalEmail { get; set; }
        public PersonForCommand Person { get; set; }
        public FunctionalRoleForCommand FunctionalRole { get; set; }
    }
}
