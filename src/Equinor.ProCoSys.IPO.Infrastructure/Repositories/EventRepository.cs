using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.MessageContracts;
using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories;

public class EventRepository : RepositoryBase<Invitation>, IEventRepository
{
    public EventRepository(IPOContext context)
        : base(context, context.Invitations,
            context.Invitations
                .Include(x => x.Participants)
                .Include(x => x.McPkgs)
                .Include(x => x.CommPkgs)
                .Include(x => x.Comments))
    {
    }

    public IInvitationEventV1 GetInvitationEvent(Guid invitationGuid)
    {
        
        //Using .Local in invitation as otherwise we get the old values from the database, and not the updated values
        //Not including project and person as joins as this created strange exception when doing sign punch out
        var invitationEvent =
            (from i in _context.Invitations.Local
             where i.Guid == invitationGuid
             select new InvitationEvent
             {
                 Guid = i.Guid,
                 ProCoSysGuid = i.Guid,
                 Plant = i.Plant,
                 ProjectName = GetProjectName(i.ProjectId),
                 Id = i.Id,
                 CreatedAtUtc = i.CreatedAtUtc,
                 CreatedByOid = GetPersonGuid(i.CreatedById), 
                 ModifiedAtUtc = i.ModifiedAtUtc,
                 Title = i.Title,
                 Type = i.Type.ToString(),
                 Description = i.Description,
                 Status = i.Status.ToString(),
                 EndTimeUtc = i.EndTimeUtc,
                 Location = i.Location,
                 StartTimeUtc = i.StartTimeUtc,
                 AcceptedAtUtc = i.AcceptedAtUtc,
                 AcceptedByOid = i.AcceptedBy == null ? null : GetPersonGuid(i.AcceptedBy.Value), 
                 CompletedAtUtc = i.CompletedAtUtc,
                 CompletedByOid = i.CompletedBy == null ? null : GetPersonGuid(i.CompletedBy.Value),
             }
            ).SingleOrDefault();

        if (invitationEvent is null)
        {
            throw new ArgumentException($"Could not find an invitation event for invitation with id {invitationGuid}");
        }

        return invitationEvent;
    }

    public ICommentEventV1 GetCommentEvent(Guid invitationGuid, Guid commentGuid)
    {
        var invitation = GetInvitationFromLocal(invitationGuid);

        var commentEvent = (from c in invitation.Comments
                join createdBy in _context.Persons on c.CreatedById equals createdBy.Id
                where c.Guid == commentGuid
                select new CommentEvent
                {
                    CommentText = c.CommentText,
                    CreatedAtUtc = c.CreatedAtUtc,
                    CreatedByOid = createdBy.Guid,
                    Plant = c.Plant,
                    InvitationGuid = invitation.Guid,
                    ProCoSysGuid = c.Guid,
                    ProjectName = GetProjectName(invitation.ProjectId)
                })
            .SingleOrDefault();
        
        if (commentEvent is null)
        {
            throw new ArgumentException($"Could not construct a comment event for invitation with id {invitationGuid} and comment id {commentGuid}");
        }

        return commentEvent;
    }

    public IParticipantEventV1 GetParticipantEvent(Guid invitationGuid, Guid participantGuid)
    {
        var invitation = GetInvitationFromLocal(invitationGuid);

        var participantEvent = (from p in invitation.Participants
                                  join createdBy in _context.Persons on p.CreatedById equals createdBy.Id
                                  join signedByInner in _context.Persons on p.SignedBy equals signedByInner.Id into signedByOuter
                                  from signedBy in signedByOuter.DefaultIfEmpty()
                                  where p.Guid == participantGuid
                                  select new ParticipantEvent()
                                  {
                                      ProCoSysGuid = p.Guid,
                                      Plant = p.Plant,
                                      ProjectName = GetProjectName(invitation.ProjectId),
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
                                      SignedByOid = signedBy?.Guid
                                  }).SingleOrDefault();

        if (participantEvent is null)
        {
            throw new ArgumentException($"Could not construct a participation event for invitation with id {invitationGuid} and participant id {participantGuid}");
        }
        return participantEvent;
    }

    public IMcPkgEventV1 GetMcPkgEvent(Guid invitationGuid, Guid mcPkgGuid)
    {
        var invitation = GetInvitationFromLocal(invitationGuid);

        var mcPkgEvent = (from m in invitation.McPkgs
                          where m.Guid.Equals(mcPkgGuid)
                          select new McPkgEvent()
                          {
                              ProCoSysGuid = m.Guid,
                              Plant = m.Plant,
                              ProjectName = GetProjectName(invitation.ProjectId),
                              InvitationGuid = invitationGuid,
                              CreatedAtUtc = m.CreatedAtUtc
                          }).SingleOrDefault();
        
        if (mcPkgEvent is null)
        {
            throw new ArgumentException($"Could not construct a mcpkg event for invitation with id {invitationGuid} and mcpkg id {mcPkgGuid}");
        }
        return mcPkgEvent;
    }

    public ICommPkgEventV1 GetCommPkgEvent(Guid invitationGuid, Guid commPkgGuid)
    {
        var invitation = GetInvitationFromLocal(invitationGuid);

        var commPkgEvent = (from m in invitation.CommPkgs
                            where m.Guid.Equals(commPkgGuid)
                            select new CommPkgEvent()
                            {
                                ProCoSysGuid = m.Guid,
                                Plant = m.Plant,
                                ProjectName = GetProjectName(invitation.ProjectId),
                                InvitationGuid = invitationGuid,
                                CreatedAtUtc = m.CreatedAtUtc
                            }).SingleOrDefault();

        if (commPkgEvent is null)
        {
            throw new ArgumentException($"Could not construct a commpkg event for invitation with id {invitationGuid} and commpkg id {commPkgGuid}");
        }

        return commPkgEvent;
    }

    private Invitation GetInvitationFromLocal(Guid invitationGuid)
    {
        var invitation = _context.Invitations.Local.SingleOrDefault(x => x.Guid.Equals(invitationGuid));
        
        if (invitation is null)
        {
            throw new ArgumentException($"Could not retrieve invitation from local with id {invitationGuid}");
        }

        return invitation;
    }


    private string GetProjectName(int projectId)
    {
        var projectName = (from p in _context.Projects
            where p.Id == projectId
            select p.Name)
            .SingleOrDefault();

        if (projectName is null)
        {
            throw new ArgumentException($"Could not retrieve project with id {projectId}");
        }

        return projectName;
    }

    private Guid GetPersonGuid(int personId)
    {
        var personGuid = (from p in _context.Persons
                where p.Id == personId
                select p.Guid)
                .SingleOrDefault();

        if (personGuid.Equals(Guid.Empty))
        {
            throw new ArgumentException($"Could not retrieve person with id {personId}");
        }

        return personGuid;
    }
}
