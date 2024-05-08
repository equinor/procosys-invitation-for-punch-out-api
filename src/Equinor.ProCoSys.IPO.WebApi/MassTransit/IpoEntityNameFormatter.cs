﻿using System;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
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
            _ => throw new ArgumentException($"IPO error: {typeof(T).Name} is not configured with a topic name mapping.")
        };
}
