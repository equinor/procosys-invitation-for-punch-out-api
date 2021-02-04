using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitations
{
    public class InvitationDto
    {
        public int IpoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DisciplineType Type { get; set; }
        public IpoStatus Status { get; set; }
        public PersonDto CreatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public DateTime? AcceptedAtUtc { get; set; }
        public string ContractorRep { get; set; }
        public string ConstructionCompanyRep { get; set; }
        public IList<string> McPkgNos { get; set; }
        public IList<string> CommPkgNos { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
