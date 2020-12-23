﻿using System.ComponentModel;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate
{
    public enum EventType
    {
        [Description("IPO completed")]
        IpoCompleted,
        [Description("IPO accepted")]
        IpoAccepted,
        [Description("IPO signed")]
        IpoSigned,
        [Description("IPO un-accepted")]
        IpoUnAccepted,
        [Description("IPO created")]
        IpoCreated,
        [Description("IPO edited")]
        IpoEdited,
        [Description("Attachment uploaded")]
        AttachmentUploaded,
        [Description("Attachment removed")]
        AttachmentRemoved
    }
}
