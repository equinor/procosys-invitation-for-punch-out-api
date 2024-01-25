using System;
using System.ComponentModel.DataAnnotations.Schema;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Participant : PlantEntityBase, ICreationAuditable, IModificationAuditable
    {
        public const int FunctionalRoleCodeMaxLength = 255;

        protected Participant()
            : base(null)
        {
        }

        public Participant(
            string plant,
            Organization organization,
            IpoParticipantType type,
            string functionalRoleCode,
            string firstName,
            string lastName,
            string userName,
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
            UserName = userName;
            Email = email;
            AzureOid = azureOid;
            SortKey = sortKey;
        }

        public Organization Organization { get; set; }
        public IpoParticipantType Type { get; set; }
        public string FunctionalRoleCode { get; set; }
        [NotMapped] // EF defines a shadow property for InvitationId.
        public int InvitationId { get; set; } // This id is needed to be able to handle relations when not using EF to load this object.
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public Guid? AzureOid { get; set; }
        public int SortKey { get; set; }
        public bool Attended { get; set; }
        public bool IsAttendedTouched { get; set; }
        public string Note { get; set; }
        public DateTime? SignedAtUtc { get; set; }
        public int? SignedBy { get; set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }
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
        public void SetModified(Person modifiedBy)
        {
            ModifiedAtUtc = TimeService.UtcNow;
            if (modifiedBy == null)
            {
                throw new ArgumentNullException(nameof(modifiedBy));
            }
            ModifiedById = modifiedBy.Id;
        }
    }
}
