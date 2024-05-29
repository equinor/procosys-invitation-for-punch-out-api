﻿using Equinor.ProCoSys.IPO.Command.Events;
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
                .Include(x => x.Comments)
                .Include(x => x.Attachments))
    {
    }

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
                 Id = i.Id,
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

        return commPkgEvent;
    }

    private Invitation GetInvitationFromLocal(Guid invitationGuid) 
        => _context.Invitations.Local.SingleOrDefault(x => x.Guid.Equals(invitationGuid));


    private string GetProjectName(int projectId) =>
        (from p in _context.Projects
            where p.Id == projectId
            select p.Name).SingleOrDefault();
}