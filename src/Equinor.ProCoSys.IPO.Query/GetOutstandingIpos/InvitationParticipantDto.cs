using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class InvitationParticipantDto
    {
        public Guid? AzureOid { get; set; }

        public string FunctionalRoleCode { get; set; }

        public Organization Organization { get; set; }

        public int? SignedBy { get; set; }
    }
}
