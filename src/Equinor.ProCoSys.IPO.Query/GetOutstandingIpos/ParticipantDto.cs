using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class ParticipantDto
    {
        public int ParticipantId { get; set; }

        public Guid? AzureOid { get; set; }

        public string FunctionalRoleCode { get; set; }

        public DateTime? SignedAtUtc { get; set; }

        public Organization Organization { get; set; }

        public int SortKey { get; set; }

        public IpoParticipantType Type { get; set; }

        public int? SignedBy { get; set; }
    }
}
