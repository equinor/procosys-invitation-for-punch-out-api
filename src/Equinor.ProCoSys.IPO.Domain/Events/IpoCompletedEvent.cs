using MediatR;

namespace Equinor.Procosys.Preservation.Domain.Events
{
    public class IpoCompletedEvent : INotification
    {
        public IpoCompletedEvent(
            string plant,
            int objectId)
        {
            Plant = plant;
            ObjectId = objectId;
        }
        public string Plant { get; }
        public int ObjectId { get; }
    }
}
