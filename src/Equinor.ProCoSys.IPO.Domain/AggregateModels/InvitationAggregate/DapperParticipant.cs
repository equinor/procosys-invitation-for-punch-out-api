using System.ComponentModel.DataAnnotations.Schema;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    /**
     * When using Dapper to read objects at various levels in the graph, we need a simple and efficient way to keep the relations.
     * This class ensures that an explicit id to the parent object can be directly mapped when reading via Dapper.
     */
    public class DapperParticipant : Participant
    {
        [NotMapped] // This annotation is used to avoid accidential use by EF.
        public int InvitationId { get; set; } // This id is needed to be able to handle relations when not using EF to load this object.
    }
}
