using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        public Project GetProject(string projectName);
        void UpdateProjectOnInvitations(string projectName, string description);
        void UpdateCommPkgOnInvitations(Guid proCoSysGuid, string commPkgNo, string description, Project project);
        void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description);
        void UpdateFunctionalRoleCodesOnInvitations(string plant, string functionalRoleCodeOld, string functionalRoleCodeNew);
        void RemoveParticipant(Participant participant);
        void RemoveAttachment(Attachment attachment);
        void RemoveInvitation(Invitation invitation);
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
