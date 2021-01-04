namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        void UpdateProjectOnInvitations(string projectName, string description);
        void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description);
        void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description);
        void RemoveCommPkg(CommPkg commPkg);
        void RemoveMcPkg(McPkg mcPkg);
        void RemoveParticipant(Participant participant);
        void RemoveAttachment(Attachment attachment);
    }
}
