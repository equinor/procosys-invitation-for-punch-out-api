using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        void UpdateProjectOnInvitations(string projectName, string description);
        void UpdateCommPkgOnInvitations(Guid commPkgGuid, string description);
        void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description);
        void UpdateFunctionalRoleCodesOnInvitations(string plant, string functionalRoleCodeOld, string functionalRoleCodeNew);
        void RemoveParticipant(Participant participant);
        void RemoveAttachment(Attachment attachment);
        void RemoveInvitation(Invitation invitation);
        bool ShouldMoveCommPkg(Guid toProjectGuid, Guid commPkgGuid);

        void MoveCommPkg(Guid toProjectGuid, Guid commPkgGuid, string description);
        void MoveMcPkg(string projectName, string fromCommPkgNo, string toCommPkgNo, string fromMcPkgNo, string toMcPkgNo, string description);
        void RfocAcceptedHandling(string projectName, IList<string> commPkgNosWithAcceptedRfoc, IList<string> mcPkgNosWithAcceptedRfoc);
        void RfocVoidedHandling(string projectName, IList<string> commPkgNos, IList<string> mcPkgNos);
        IList<Invitation> GetInvitationsForSynchronization();
        IList<CommPkg> GetCommPkgs(string projectName, IList<string> commPkgNos);
        IList<McPkg> GetMcPkgs(string projectName, IList<string> mcPkgNos);
        IList<CommPkg> GetCommPkgsOnly();
        IList<McPkg> GetMcPkgsOnly();
    }
}
