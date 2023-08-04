using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs;

public class OutstandingIpoDto
{
    public int Id { get; set; }
    public string Description { get; set; }

    public IpoStatus Status { get; set; }
    public Organization Organization { get; set; }
    public int? SignedBy { get; set; }
    public Guid? AzureOid { get; set; }
    public string FunctionalRoleCode { get; set; }
}
