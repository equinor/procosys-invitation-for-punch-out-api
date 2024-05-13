using System;
using Equinor.ProCoSys.IPO.MessageContracts.Invitation;

namespace Equinor.ProCoSys.IPO.Command.Events.Invitation;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

public record InvitationCreatedIntegrationEvent
(
    //Guid Guid,
    //Guid ProCoSysGuid,
    //string Plant,
    //string ProjectName,
    //string IpoNumber,
    //DateTime CreatedAtUtc,
    //Guid CreatedByOid,
    //DateTime? ModifiedAtUtc,
    //string Title,
    //string Type,
    //string Description,
    //string Status,
    //DateTime EndTimeUtc,
    //string Location,
    //DateTime StartTimeUtc,
    //DateTime? AcceptedAtUtc,
    //Guid AcceptedByOid,
    //DateTime? CompletedAtUtc,
    //Guid CompletedByOid

) : IInvitationCreatedEventV1

{
    public Guid Guid { get; set; }
    public Guid ProCoSysGuid { get; init; }
    public string Plant { get; init; }
    public string ProjectName { get; init; }
    public string IpoNumber { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid CreatedByOid { get; init; }
    public DateTime? ModifiedAtUtc { get; init; }
    public string Title { get; init; }
    public string Type { get; init; }
    public string Description { get; init; }
    public string Status { get; init; }
    public DateTime EndTimeUtc { get; init; }
    public string Location { get; init; }
    public DateTime StartTimeUtc { get; init; }
    public DateTime? AcceptedAtUtc { get; init; }
    public Guid AcceptedByOid { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public Guid CompletedByOid { get; init; }
}
//{
    //public InvitationCreatedIntegrationEvent(Invitation invitation) : this(

    //    invitation.Guid,
    //    invitation.Plant,
    //    invitation.Project.ProjectName,
    //    invitation.IpoNumber,
    //    invitation.CreatedAtUtc,
    //    invitation.CreatedByOid,
    //    invitation.ModifiedAtUtc,
    //    invitation.Title,
    //    invitation.Type,
    //    invitation.Description,
    //    invitation.NotificationStatus,
    //    invitation.EndTimeUtc,
    //    invitation.Location,
    //    invitation.StartTimeUtc,
    //    invitation.AcceptedAtUtc,
    //    invitation.AcceptedByGuidOid,
    //    invitation.CompletedByUtc,
    //    invitation.CompletedByOid
    //)
    //{
    //}
//}

