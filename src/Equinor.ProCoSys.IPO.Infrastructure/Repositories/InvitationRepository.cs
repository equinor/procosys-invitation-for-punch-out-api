using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.MessageContracts;
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

        public IList<Invitation> GetInvitationsForSynchronization() =>
            _context.Invitations
                .Include(i => i.McPkgs)
                .Include(i => i.CommPkgs)
                .Where(i => i.Status == IpoStatus.ScopeHandedOver)
                .ToList();

        public void UpdateProjectOnInvitations(string projectName, string description)
        {
            //Intentionally left blank for now
        }

        public void UpdateCommPkgOnInvitations(string projectName, string commPkgNo, string description)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            var commPkgsToUpdate = _context.CommPkgs.Where(cp => project != null && cp.ProjectId == project.Id && cp.CommPkgNo == commPkgNo).ToList();

            commPkgsToUpdate.ForEach(cp => cp.Description = description);
        }

        public void MoveCommPkg(string fromProjectName, string toProjectName, string commPkgNo, string description)
        {
            var toProject = _context.Projects.SingleOrDefault(x => x.Name.Equals(toProjectName));
            var fromProject = _context.Projects.SingleOrDefault(x => x.Name.Equals(fromProjectName));

            var commPkgsToMove = _context.CommPkgs.Where(cp => fromProject != null && cp.ProjectId == fromProject.Id && cp.CommPkgNo == commPkgNo).ToList();

            var mcPkgsToMove = _context.McPkgs.Where(mc => fromProject != null && mc.ProjectId == fromProject.Id && mc.CommPkgNo == commPkgNo).ToList();

            var invitationsToMove =
                _context.Invitations
                    .Where(i => fromProject != null && i.ProjectId == fromProject.Id &&
                                (i.CommPkgs.Any(c => c.CommPkgNo == commPkgNo) || i.McPkgs.Any(m => m.CommPkgNo == commPkgNo))).ToList();

            if (InvitationsContainMoreThanOneCommPkg(invitationsToMove) || NotAllMcPkgsOnInvitationsBelongToGivenCommPkg(commPkgNo, invitationsToMove))
            { 
                throw new Exception($"Unable to move to other comm pkg {commPkgNo } to {toProjectName}. Will result in bad data as invitation will reference more than one project");
            }

            invitationsToMove.ForEach(i =>
            {
                i.MoveToProject(toProject);
            });

            commPkgsToMove.ForEach(cp =>
            {
                cp.Description = description;
                cp.MoveToProject(toProject);
            });

            mcPkgsToMove.ForEach(mc =>
            {
                mc.MoveToProject(toProject);
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
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            var mcPkgsToUpdate = _context.McPkgs.Where(mp => project != null && mp.ProjectId == project.Id && mp.CommPkgNo == fromCommPkgNo && mp.McPkgNo == fromMcPkgNo).ToList();

            mcPkgsToUpdate.ForEach(mp =>
            {
                mp.MoveToCommPkg(toCommPkgNo);
                mp.Rename(toMcPkgNo);
                mp.Description = description;
            });
        }

        public void UpdateMcPkgOnInvitations(string projectName, string mcPkgNo, string description)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            var mcPkgsToUpdate = _context.McPkgs.Where(mp => project != null && mp.ProjectId == project.Id && mp.McPkgNo == mcPkgNo).ToList();

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
                //JSOI ParticipantRemoved domain event
            }
            _context.Invitations.Remove(invitation);
            //JSOI invitationremoved domain event
        }

        public void RfocVoidedHandling(string projectName, IList<string> commPkgNos, IList<string> mcPkgNos)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            if (project == null)
            {
                throw new NullReferenceException($"Project not found. {projectName}.");
            }

            var invitations = _context.Invitations
                   .Include(i => i.McPkgs)
                   .Include(i => i.CommPkgs)
                   .ToList()
                   .Where(i => i.ProjectId == project.Id && i.Status is IpoStatus.ScopeHandedOver
                           && (i.CommPkgs.Any(x => commPkgNos.Any(y => y == x.CommPkgNo))
                               || i.McPkgs.Any(x => mcPkgNos.Any(y => y == x.McPkgNo))))
                   .ToList();

            foreach (var invitation in invitations)
            {
                if (invitation.Type == DisciplineType.MDP)
                {
                    UpdateRfocAcceptedForMdp(invitation, commPkgNos, false);
                    if (!invitation.CommPkgs.All(c => c.RfocAccepted))
                    {
                        invitation.ResetStatus();
                    }
                }
                else
                {
                    UpdateRfocAcceptedForDp(invitation, mcPkgNos, false);
                    if (!invitation.McPkgs.All(c => c.RfocAccepted))
                    {
                        invitation.ResetStatus();
                    }
                }
            }
        }

        public void RfocAcceptedHandling(string projectName, IList<string> commPkgNosWithAcceptedRfoc, IList<string> mcPkgNosWithAcceptedRfoc)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            if (project == null)
            {
                throw new NullReferenceException($"Project not found. {projectName}.");
            }

            var invitations = _context.Invitations
                    .Include(i => i.McPkgs)
                    .Include(i => i.CommPkgs)
                    .ToList()
                    .Where(i => i.ProjectId == project.Id && i.Status is IpoStatus.Completed or IpoStatus.Planned or IpoStatus.Accepted
                            && (i.CommPkgs.Any(x => commPkgNosWithAcceptedRfoc.Any(y => y == x.CommPkgNo))
                                || i.McPkgs.Any(x => mcPkgNosWithAcceptedRfoc.Any(y => y == x.McPkgNo))))
                    .ToList();

            foreach (var invitation in invitations)
            {
                if (invitation.Type == DisciplineType.MDP)
                {
                    UpdateRfocAcceptedForMdp(invitation, commPkgNosWithAcceptedRfoc, true);
                    if (invitation.CommPkgs.All(c => c.RfocAccepted))
                    {
                        invitation.ScopeHandedOver();
                    }
                }
                else
                {
                    UpdateRfocAcceptedForDp(invitation, mcPkgNosWithAcceptedRfoc, true);
                    if (invitation.McPkgs.All(c => c.RfocAccepted))
                    {
                        invitation.ScopeHandedOver();
                    }
                }
            }
        }

        public IList<CommPkg> GetCommPkgsOnly()
            => _context.CommPkgs.ToList();

        public IList<McPkg> GetMcPkgsOnly()
            => _context.McPkgs.ToList();

        public IList<CommPkg> GetCommPkgs(string projectName, IList<string> commPkgNos)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            if (project == null)
            {
                throw new NullReferenceException($"Project not found. {projectName}.");
            }

            return _context.CommPkgs.Where(c => commPkgNos.Contains(c.CommPkgNo) && c.ProjectId == project.Id).ToList();
        }

        public IList<McPkg> GetMcPkgs(string projectName, IList<string> mcPkgNos)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Name.Equals(projectName));

            if (project == null)
            {
                throw new NullReferenceException($"Project not found. {projectName}.");
            }

            return _context.McPkgs.Where(mc => mcPkgNos.Contains(mc.McPkgNo) && mc.ProjectId == project.Id).ToList();
        }

        private void UpdateRfocAcceptedForMdp(Invitation invitation, IList<string> commPkgNos, bool rfocAccepted) =>
            invitation.CommPkgs.Where(c => commPkgNos.Contains(c.CommPkgNo)).ToList()
                .ForEach(c => c.RfocAccepted = rfocAccepted);

        private void UpdateRfocAcceptedForDp(Invitation invitation, IList<string> mcPkgNos, bool rfocAccepted) =>
            invitation.McPkgs.Where(mc => mcPkgNos.Contains(mc.McPkgNo)).ToList()
                .ForEach(mc => mc.RfocAccepted = rfocAccepted);


        public IInvitationEventV1 GetInvitationEvent(Guid invitationGuid)
        {
            //Using .Local as otherwise we get the old values from the database, and not the updated values
            var result =
                (from i in _context.Invitations.Local
                    join project in _context.Projects on i.ProjectId equals project.Id
                    join completedByInner in _context.Persons on i.CompletedBy equals completedByInner.Id into completedByOuter
                    from completedBy in completedByOuter.DefaultIfEmpty()
                    join acceptedByInner in _context.Persons on i.AcceptedBy equals acceptedByInner.Id into acceptedByOuter
                    from acceptedBy in acceptedByOuter.DefaultIfEmpty()
                    join createdByInner in _context.Persons on i.CreatedById equals createdByInner.Id into createdByOuter
                    from createdBy in createdByOuter.DefaultIfEmpty()
                    where i.Guid == invitationGuid
                    select new InvitationEvent
                    {
                        Guid = i.Guid,
                        ProCoSysGuid = i.Guid,
                        Plant = i.Plant,
                        ProjectName = project.Name,
                        IpoNumber = "IPO - " + i.Id,
                        CreatedAtUtc = i.CreatedAtUtc,
                        CreatedByOid = createdBy.Guid,
                        ModifiedAtUtc = i.ModifiedAtUtc,
                        Title = i.Title,
                        Type = i.Type.ToString(),
                        Description = i.Description,
                        Status = i.Status.ToString(),
                        EndTimeUtc = i.EndTimeUtc,
                        Location = i.Location,
                        StartTimeUtc = i.StartTimeUtc,
                        AcceptedAtUtc = i.AcceptedAtUtc,
                        AcceptedByOid = acceptedBy?.Guid,
                        CompletedAtUtc = i.CompletedAtUtc,
                        CompletedByOid = completedBy?.Guid,
                    }
                ).SingleOrDefault();

            if (result is null)
            {
                throw new ArgumentException($"Could not find an invitation event for invitation with id {invitationGuid}");
            }

            return result;
        }

        public ICommentEventV1 GetCommentEvent(Guid invitationGuid, Guid commentGuid)
        {
            //TODO: Fix possible nullpointers
            var invitation = (from i in _context.Invitations
                    where i.Guid.Equals(invitationGuid)
                    select i)
                .SingleOrDefault();

            var comment = (from c in invitation.Comments
                    join createdBy in _context.Persons on c.CreatedById equals createdBy.Id
                    where c.Guid == commentGuid
                    select new
                    {
                        CommentText = c.CommentText,
                        CreatedAtUtc = c.CreatedAtUtc,
                        CreatedByGuid = createdBy.Guid,
                        Plant = c.Plant,
                        ProCoSysGuid = c.Guid,
                    })
                .Single();

            var project = (from p in _context.Projects
                where p.Id == invitation.ProjectId
                select p).SingleOrDefault();

            var commentEvent = new CommentEvent
            {
                CommentText = comment.CommentText,
                CreatedAtUtc = comment.CreatedAtUtc,
                CreatedByGuid = comment.CreatedByGuid,
                Plant = comment.Plant,
                InvitationGuid = invitation.Guid,
                ProCoSysGuid = comment.ProCoSysGuid,
                ProjectName = project.Name
            };

            return commentEvent;
        }

        public IParticipantEventV1 GetParticipantEvent(Guid invitationGuid, Guid participantGuid)
        {
            //TODO: Handle null reference
            var invitation = (from i in _context.Invitations.Local
                    where i.Guid.Equals(invitationGuid)
                    select i)
                .SingleOrDefault();

            if (invitation is null)
            {
                throw new ArgumentException($"Could not find an invitation for invitation with id {invitationGuid} and participant id {participantGuid}");
            }

            //TODO: Consider using QuerySet in order to get AsNoTracking functionality
            var projectName = (from p in _context.Projects
                                                where p.Id == invitation.ProjectId
                                                select p.Name).SingleOrDefault();

            if (projectName is null)
            {
                throw new ArgumentException($"Could not find a project for invitation with id {invitationGuid}");
            }

            var participantMessage = (from p in invitation.Participants
                join createdBy in _context.Persons on p.CreatedById equals createdBy.Id
                join signedByInner in _context.Persons on p.SignedBy equals signedByInner.Id into signedByOuter
                from signedBy in signedByOuter.DefaultIfEmpty()
                where p.Guid == participantGuid
                select new ParticipantEvent()
                {
                    ProCoSysGuid = p.Guid,
                    Plant = p.Plant,
                    ProjectName = projectName,
                    Organization = p.Organization.ToString(),
                    Type = p.Type.ToString(),
                    FunctionalRoleCode = p.FunctionalRoleCode,
                    AzureOid = p.AzureOid,
                    SortKey = p.SortKey,
                    CreatedAtUtc = p.CreatedAtUtc,
                    InvitationGuid = invitation.Guid,
                    ModifiedAtUtc = p.ModifiedAtUtc,
                    Attended = p.Attended,
                    Note = p.Note,
                    SignedAtUtc = p.SignedAtUtc,
                    SignedByOid = signedBy != null ? signedBy.Guid : null
                }).SingleOrDefault();

            if (participantMessage is null)
            {
                throw new ArgumentException($"Could not find an participation event for invitation with id {invitationGuid} and participant id {participantGuid}");
            }
            return participantMessage;
        }
    }
}
