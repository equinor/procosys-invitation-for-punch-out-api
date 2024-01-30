using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.ExportIPOs;

/**
 * When using Dapper to read objects at various levels in the graph, we need a simple and efficient way to keep the relations.
 * This class ensures that an explicit id to the parent object can be directly mapped when reading via Dapper.
 */
public class DapperCommPkg : CommPkg
{
    public int InvitationId { get; set; }
}
