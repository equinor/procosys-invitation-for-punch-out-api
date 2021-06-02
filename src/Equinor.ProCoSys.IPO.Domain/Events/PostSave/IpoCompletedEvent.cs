using System;
using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.IPO.Domain.Events.PostSave
{
    public class IpoCompletedEvent : INotification
    {
        public IpoCompletedEvent(
            string plant,
            Guid objectGuid,
            int id,
            string title,
            List<string> emails)
        {
            Plant = plant;
            ObjectGuid = objectGuid;
            Id = id;
            Title = title;
            Emails = emails;
        }
        public string Plant { get; }
        public Guid ObjectGuid { get; }
        public int Id { get; }
        public string Title { get; }
        public List<string> Emails { get; }
    }
}
