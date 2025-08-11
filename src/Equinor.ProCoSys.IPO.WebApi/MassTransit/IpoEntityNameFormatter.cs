using System;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.MessageContracts;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MassTransit;

namespace Equinor.ProCoSys.IPO.WebApi.MassTransit;

public class IpoEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>() =>
        typeof(T).Name switch
        {
            nameof(BusEventMessage) => IpoTopic.TopicName,
            //MassTransit calls this formatter with both BusEventMessage and IIntegrationEvent.
            //Handling it so it does not crash the application.
            nameof(IIntegrationEvent) => nameof(IIntegrationEvent),
            nameof(IInvitationEventV1) => "ipoinvitation",
            nameof(InvitationEvent) => "ipoinvitation",
            nameof(IDeleteEventV1) => nameof(IDeleteEventV1),
            nameof(InvitationDeleteEvent) => "ipoinvitation",
            nameof(CommentDeleteEvent) => "ipoinvitationcomment",
            nameof(ParticipantDeleteEvent) => "ipoinvitationparticipant",
            nameof(ICommentEventV1) => "ipoinvitationcomment",
            nameof(CommentEvent) => "ipoinvitationcomment",
            nameof(IParticipantEventV1) => "ipoinvitationparticipant",
            nameof(ParticipantEvent) => "ipoinvitationparticipant",
            nameof(IMcPkgEventV1) => "ipoinvitationmcpkg",
            nameof(McPkgEvent) => "ipoinvitationmcpkg",
            nameof(McPkgDeleteEvent) => "ipoinvitationmcpkg",
            nameof(ICommPkgEventV1) => "ipoinvitationcommpkg",
            nameof(CommPkgEvent) => "ipoinvitationcommpkg",
            nameof(CommPkgDeleteEvent) => "ipoinvitationcommpkg",

            _ => throw new ArgumentException($"IPO error: {typeof(T).Name} is not configured with a topic name mapping.")
        };
}
