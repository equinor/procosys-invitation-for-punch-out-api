using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

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
            //var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.ProjectName == projectName).ToList();

            //mcPkgsToUpdate.ForEach(mp => mp.P = description);
        }

        public void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description)
        {
            var commPkgsToUpdate = _context.CommPkgs.Where(cp => cp.ProjectName == projectName && cp.CommPkgNo == commPkgNo).ToList();

            commPkgsToUpdate.ForEach(cp => cp.Description = description);
        }

        public void MoveCommPkg(string fromProject, string toProject, string commPkgNo, string description)
        {
            var commPkgsToMove = _context.CommPkgs.Where(cp => cp.ProjectName == fromProject && cp.CommPkgNo == commPkgNo).ToList();

            commPkgsToMove.ForEach(cp =>
            {
                cp.Description = description;
                cp.MoveTo(toProject);
            });

        }

        public void MoveMcPkg(
            string projectName,
            string fromCommPkgNo,
            string toCommPkgNo,
            string fromMcPkgNo,
            string toMcPkgNo,
            string description)
        {
            var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.ProjectName == projectName && mp.CommPkgNo == fromCommPkgNo && mp.McPkgNo == fromMcPkgNo).ToList();

            mcPkgsToUpdate.ForEach(mp =>
            {
                mp.MoveToCommPkg(toCommPkgNo);
                mp.Rename(toMcPkgNo);
                mp.Description = description;
            });
        }

        public void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description)
        {
            var mcPkgsToUpdate = _context.McPkgs.Where(mp => mp.ProjectName == projectName && mp.McPkgNo == mcPkgNo).ToList();

            mcPkgsToUpdate.ForEach(mp => mp.Description=description);
        }

        public void RemoveCommPkg(CommPkg commPkg)
            => _context.CommPkgs.Remove(commPkg);    
        
        public void RemoveMcPkg(McPkg mcPkg)
            => _context.McPkgs.Remove(mcPkg);

        public void RemoveParticipant(Participant participant)
            => _context.Participants.Remove(participant);

        public void RemoveAttachment(Attachment attachment)
            => _context.Attachments.Remove(attachment);

        public void RemoveComment(Comment comment)
            => _context.Comments.Remove(comment);

    }
}
