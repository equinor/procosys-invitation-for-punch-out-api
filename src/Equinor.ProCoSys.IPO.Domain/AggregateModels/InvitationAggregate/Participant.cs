using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Participant : PlantEntityBase, ICreationAuditable
    {
        public const int FunctionalRoleCodeMaxLength = 255;
        public const int FirstNameMaxLength = 255;
        public const int LastNameMaxLength = 255;

        protected Participant()
            : base(null)
        {
        }

        public Participant(
            string plant, 
            Organization organization, 
            ParticipantType type, 
            string functionalRoleCode, 
            string firstName, 
            string lastName, 
            string email,
            Guid? azureOid,
            int sortKey)
            : base(plant)
        {
            Organization = organization;
            Type = type;
            FunctionalRoleCode = functionalRoleCode;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            AzureOid = azureOid;
            SortKey = sortKey;
        }

        public Organization Organization { get; private set; }
        public ParticipantType Type { get; private set; }
        public string FunctionalRoleCode { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public Guid? AzureOid { get; private set; }
        public int SortKey { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }
    }
}
