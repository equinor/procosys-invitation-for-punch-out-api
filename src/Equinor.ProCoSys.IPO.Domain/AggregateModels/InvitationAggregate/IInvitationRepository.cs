namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        void RemoveCommPkg(CommPkg commPkg);
        void RemoveMcPkg(McPkg mcPkg);
        void RemoveParticipant(Participant participant);
    }
}
