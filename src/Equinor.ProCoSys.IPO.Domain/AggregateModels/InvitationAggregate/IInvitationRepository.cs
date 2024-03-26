using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        void UpdateProjectOnInvitations(string projectName, string description);
        void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description, Guid commPkgGuid);
        void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description, Guid mcPkgGuid, Guid commPkgGuid);
        void UpdateFunctionalRoleCodesOnInvitations(string plant, string functionalRoleCodeOld, string functionalRoleCodeNew);
        void RemoveParticipant(Participant participant);
        void RemoveAttachment(Attachment attachment);
        void RemoveInvitation(Invitation invitation);
        void MoveCommPkg(string fromProject, string toProject, string commPkgNo, string description);
        void RfocAcceptedHandling(string projectName, IList<string> commPkgNosWithAcceptedRfoc, IList<string> mcPkgNosWithAcceptedRfoc);
        void RfocVoidedHandling(string projectName, IList<string> commPkgNos, IList<string> mcPkgNos);
        IList<Invitation> GetInvitationsForSynchronization();
        IList<CommPkg> GetCommPkgs(string projectName, IList<string> commPkgNos);
        IList<McPkg> GetMcPkgs(string projectName, IList<string> mcPkgNos);
        IList<CommPkg> GetCommPkgsOnly();
        IList<McPkg> GetMcPkgsOnly();
    }
}
