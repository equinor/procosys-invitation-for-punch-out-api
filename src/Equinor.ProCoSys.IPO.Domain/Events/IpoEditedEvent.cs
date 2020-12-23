﻿using System;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events
{
    public class IpoEditedEvent : INotification
    {
        public IpoEditedEvent(
            string plant,
            Guid objectGuid)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
    }
}
