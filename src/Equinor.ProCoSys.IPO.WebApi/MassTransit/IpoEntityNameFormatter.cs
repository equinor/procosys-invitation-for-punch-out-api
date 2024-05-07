using System;
using Equinor.ProCoSys.IPO.Command.EventHandlers.PostSaveEvents;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MassTransit;

namespace Equinor.ProCoSys.IPO.WebApi.MassTransit;

public class IpoEntityNameFormatter : IEntityNameFormatter
{
    //TODO: JSOI Unit tests
    public string FormatEntityName<T>() =>
        typeof(T).Name switch
        {
            nameof(BusEventMessage) => IpoTopic.TopicName,
            _ => throw new ArgumentException($"{typeof(T).Name} is not supported")
        };
}
