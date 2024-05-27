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
            nameof(IIntegrationEvent) => nameof(IIntegrationEvent),//nameof(IIntegrationEvent),
            nameof(IInvitationEventV1) => "ipoinvitation", //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(InvitationEvent) => "ipoinvitation", //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(IDeleteEventV1) => nameof(IDeleteEventV1), //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(DeleteEvent) => nameof(DeleteEvent), //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(InvitationDeleteEvent) => "ipoinvitation",
            nameof(CommentDeleteEvent) => "ipocomment",
            nameof(ParticipantDeleteEvent) => "ipoparticipant",
            nameof(ICommentEventV1) => "ipocomment", //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(CommentEvent) => "ipocomment", //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(IParticipantEventV1) => "ipoparticipant", //TODO: JSOI Replace with IpoTopic name from ServiceBus
            nameof(ParticipantEvent) => "ipoparticipant", //TODO: JSOI Replace with IpoTopic name from ServiceBus
            _ => throw new ArgumentException($"IPO error: {typeof(T).Name} is not configured with a topic name mapping.")
        };
}
