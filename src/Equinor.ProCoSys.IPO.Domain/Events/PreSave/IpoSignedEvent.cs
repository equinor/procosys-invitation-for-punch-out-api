﻿using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class IpoSignedEvent : DomainEvent
    {
        public IpoSignedEvent(
            string plant,
            Guid sourceGuid) : base("IPO signed")
        {
            Plant = plant;
            SourceGuid = sourceGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
    }
}
