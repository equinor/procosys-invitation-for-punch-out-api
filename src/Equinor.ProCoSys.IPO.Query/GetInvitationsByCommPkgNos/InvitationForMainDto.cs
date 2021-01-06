using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNos
{
    public class InvitationForMainDto
    {
        public InvitationForMainDto(
            int id,
            string title,
            DisciplineType type,
            IpoStatus status,
            string rowVersion)
        {
            Id = id;
            Title = title;
            Type = type;
            Status = status;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string Title { get; }
        public DisciplineType Type { get; }
        public IpoStatus Status { get; }
        public string RowVersion { get; }
    }
}
