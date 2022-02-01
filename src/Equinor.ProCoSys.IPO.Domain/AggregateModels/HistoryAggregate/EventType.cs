using System.ComponentModel;

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
        [Description("IPO unsigned")]
        IpoUnsigned,
        [Description("IPO uncompleted")]
        IpoUncompleted,
        [Description("IPO unaccepted")]
        IpoUnaccepted,
        [Description("IPO created")]
        IpoCreated,
        [Description("IPO edited")]
        IpoEdited,
        [Description("Attachment uploaded")]
        AttachmentUploaded,
        [Description("Attachment removed")]
        AttachmentRemoved,
        [Description("Comment added")]
        CommentAdded,
        [Description("Comment removed")]
        CommentRemoved,
        [Description("IPO canceled")]
        IpoCanceled
    }
}
