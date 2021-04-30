namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        void UpdateProjectOnInvitations(string projectName, string description);
        void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description);
        void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description);
        void RemoveParticipant(Participant participant);
        void RemoveAttachment(Attachment attachment);
        void MoveCommPkg(string fromProject, string toProject, string commPkgNo, string description);
        void MoveMcPkg(string projectName, string fromCommPkgNo, string toCommPkgNo, string fromMcPkgNo, string toMcPkgNo, string description);
    }
}
