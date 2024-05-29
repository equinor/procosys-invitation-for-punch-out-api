using System;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.MessageContracts;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MassTransit;

namespace Equinor.ProCoSys.IPO.WebApi.MassTransit;

public class IpoEntityNameFormatter : IEntityNameFormatter
{
    //TODO: JSOI Replace hardcoded strings with IpoTopic name from ServiceBus solution?
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
            nameof(CommentDeleteEvent) => "ipocomment",
            nameof(ParticipantDeleteEvent) => "ipoparticipant",
            nameof(ICommentEventV1) => "ipocomment", 
            nameof(CommentEvent) => "ipocomment", 
            nameof(IParticipantEventV1) => "ipoparticipant", 
            nameof(ParticipantEvent) => "ipoparticipant", 
            nameof(IMcPkgEventV1) => "ipomcpkg", 
            nameof(McPkgEvent) => "ipomcpkg", 
            nameof(McPkgDeleteEvent) => "ipomcpkg",
            nameof(ICommPkgEventV1) => "ipocommpkg", 
            nameof(CommPkgEvent) => "ipocommpkg", 
            nameof(CommPkgDeleteEvent) => "ipocommpkg",

            _ => throw new ArgumentException($"IPO error: {typeof(T).Name} is not configured with a topic name mapping.")
        };
}
