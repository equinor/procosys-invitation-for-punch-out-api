using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Invitation : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
    {
        private Invitation()
        {
        }

        public Invitation()
        {

        }

        public DateTime CreatedAtUtc => throw new NotImplementedException();

        public int CreatedById => throw new NotImplementedException();

        public DateTime? ModifiedAtUtc => throw new NotImplementedException();

        public int? ModifiedById => throw new NotImplementedException();

        public void SetCreated(Person createdBy)
        {
            throw new NotImplementedException();
        }

        public void SetModified(Person modifiedBy)
        {
            throw new NotImplementedException();
        }
    }
}
