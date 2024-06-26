﻿using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.Events.PreSave
{
    public class CommentRemovedEvent : IDomainEvent
    {
        public CommentRemovedEvent(string plant, Guid sourceGuid, Guid commentGuid)
        {
            Plant = plant;
            SourceGuid = sourceGuid;
            CommentGuid = commentGuid;
        }
        public string Plant { get; }
        public Guid SourceGuid { get; }
        public Guid CommentGuid { get; }
    }
}
