using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public interface IInvitationRepository : IRepository<Invitation>
    {
        void UpdateProjectOnInvitations(string projectName, string description);
        void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description);
        void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description);
        void UpdateFunctionalRoleCodesOnInvitations(string plant, string functionalRoleCodeOld, string functionalRoleCodeNew);
        void RemoveParticipant(Participant participant);
        void RemoveAttachment(Attachment attachment);
        void RemoveInvitation(Invitation invitation);
        void MoveCommPkg(string fromProject, string toProject, string commPkgNo, string description);
        void MoveMcPkg(string projectName, string fromCommPkgNo, string toCommPkgNo, string fromMcPkgNo, string toMcPkgNo, string description);
        void UpdateRfocStatuses(string projectName, IList<string> commPkgNos, IList<Tuple<string, string>> mcPkgs);
        IList<Invitation> GetInvitationsForSynchronization();
        IList<CommPkg> GetCommPkgs(string projectName, IList<string> commPkgNos);
        McPkg GetMcPkg(string projectName, string commPkgNo, string mcPkgNo);
    }
}
