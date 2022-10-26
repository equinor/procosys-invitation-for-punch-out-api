using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class InvitationRepository : RepositoryBase<Invitation>, IInvitationRepository
    {
        public InvitationRepository(IPOContext context)
            : base(context, context.Invitations, 
                context.Invitations
                    .Include(x => x.Participants)
                    .Include(x => x.McPkgs)
                    .Include(x => x.CommPkgs)
                    .Include(x => x.Comments)
                    .Include(x => x.Attachments))
        {
        }

        public void UpdateProjectOnInvitations(string projectName, string description)
        {
            //Intentionally left blank for now
        }

        public void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description)
        {
            //var commPkgsToUpdate = _context.CommPkgs.Where(cp => cp.Project.Name == projectName && cp.CommPkgNo == commPkgNo).ToList();
            
            var projectEntity = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            var commPkgsToUpdate = _context.CommPkgs.Where(cp => cp.ProjectId == projectEntity.Id && cp.CommPkgNo == commPkgNo).ToList();

            commPkgsToUpdate.ForEach(cp => cp.Description = description);
        }

        public void MoveCommPkg(string fromProject, string toProject, string commPkgNo, string description)
        {
            var toProjectEntity = _context.Projects.SingleOrDefault(x => x.Name.Equals(toProject)); //TODO: JSOI Need to filter on Plant as well to get correct Project?
            var fromProjectEntity = _context.Projects.SingleOrDefault(x => x.Name.Equals(fromProject)); //TODO: JSOI Need to filter on Plant as well to get correct Project?

            //var commPkgsToMove = _context.CommPkgs.Where(cp => cp.Project.Name == fromProject && cp.CommPkgNo == commPkgNo).ToList();
            var commPkgsToMove = _context.CommPkgs.Where(cp => fromProjectEntity != null && cp.ProjectId == fromProjectEntity.Id && cp.CommPkgNo == commPkgNo).ToList();

            //var mcPkgsToMove = _context.McPkgs.Where(mc => mc.Project.Name == fromProject && mc.CommPkgNo == commPkgNo).ToList();
            var mcPkgsToMove = _context.McPkgs.Where(mc => fromProjectEntity != null && mc.ProjectId == fromProjectEntity.Id && mc.CommPkgNo == commPkgNo).ToList();

            //var invitationsToMove =
            //    _context.Invitations
            //        .Where(i => i.Project.Name == fromProject &&
            //                    (i.CommPkgs.Any(c => c.CommPkgNo == commPkgNo) || i.McPkgs.Any(m => m.CommPkgNo == commPkgNo))).ToList();

            var invitationsToMove =
                _context.Invitations
                    .Where(i => fromProjectEntity != null && i.ProjectId == fromProjectEntity.Id &&
                                (i.CommPkgs.Any(c => c.CommPkgNo == commPkgNo) || i.McPkgs.Any(m => m.CommPkgNo == commPkgNo))).ToList();

            if (InvitationsContainMoreThanOneCommPkg(invitationsToMove) || NotAllMcPkgsOnInvitationsBelongToGivenCommPkg(commPkgNo, invitationsToMove))
            { 
                throw new Exception($"Unable to move to other comm pkg {commPkgNo } to {toProject}. Will result in bad data as invitation will reference more than one project");
            }

            invitationsToMove.ForEach(i =>
            {
                i.MoveToProject(toProjectEntity);
            });

            commPkgsToMove.ForEach(cp =>
            {
                cp.Description = description;
                cp.MoveToProject(toProjectEntity);
            });

            mcPkgsToMove.ForEach(mc =>
            {
                mc.MoveToProject(toProjectEntity);
            });
        }

        private static bool NotAllMcPkgsOnInvitationsBelongToGivenCommPkg(string commPkgNo, List<Invitation> invitationsToMove) => invitationsToMove.Any(i => i.McPkgs.Any(m => m.CommPkgNo!=commPkgNo));

        private static bool InvitationsContainMoreThanOneCommPkg(List<Invitation> invitationsToMove) => invitationsToMove.Any(i => i.CommPkgs.Count()>1);

        public void MoveMcPkg(
            string projectName,
            string fromCommPkgNo,
            string toCommPkgNo,
            string fromMcPkgNo,
            string toMcPkgNo,
            string description)
        {
            var projectEntity = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName)); //TODO: JSOI Need to filter on Plant as well to get correct Project?

            //var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.Project.Name == projectName && mp.CommPkgNo == fromCommPkgNo && mp.McPkgNo == fromMcPkgNo).ToList();
            var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.ProjectId == projectEntity.Id && mp.CommPkgNo == fromCommPkgNo && mp.McPkgNo == fromMcPkgNo).ToList();

            mcPkgsToUpdate.ForEach(mp =>
            {
                mp.MoveToCommPkg(toCommPkgNo);
                mp.Rename(toMcPkgNo);
                mp.Description = description;
            });
        }

        public void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description)
        {
            var projectEntity = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName)); //TODO: JSOI Need to filter on Plant as well to get correct Project?

            //var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.Project.Name == projectName && mp.McPkgNo == mcPkgNo).ToList();
            var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.ProjectId == projectEntity.Id && mp.McPkgNo == mcPkgNo).ToList();

            mcPkgsToUpdate.ForEach(mp => mp.Description=description);
        }

        public void UpdateFunctionalRoleCodesOnInvitations(string plant, string functionalRoleCodeOld, string functionalRoleCodeNew)
        {
            var invitationsToUpdate = _context.Invitations
                .Include(i => i.Participants)
                .Where(invitation => invitation.Participants
                .Any(p => p.FunctionalRoleCode == functionalRoleCodeOld) && invitation.Plant == plant);

            foreach (var invitation in invitationsToUpdate)
            {
                invitation.Participants.Where(p => p.FunctionalRoleCode == functionalRoleCodeOld).ToList()
                    .ForEach(p => p.FunctionalRoleCode = functionalRoleCodeNew);
            }
        }

        public void RemoveParticipant(Participant participant)
            => _context.Participants.Remove(participant);

        public void RemoveAttachment(Attachment attachment)
            => _context.Attachments.Remove(attachment);

        public void RemoveInvitation(Invitation invitation)
        {
            foreach (var attachment in invitation.Attachments)
            {
                RemoveAttachment(attachment);
            }
            foreach (var participant in invitation.Participants)
            {
                RemoveParticipant(participant);
            }
            _context.Invitations.Remove(invitation);
        }
    }
}
