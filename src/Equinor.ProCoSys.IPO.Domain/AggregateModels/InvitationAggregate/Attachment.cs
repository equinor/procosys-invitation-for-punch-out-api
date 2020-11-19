using System;
using System.IO;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Attachment : PlantEntityBase, ICreationAuditable, IModificationAuditable
    {
        public const int FileNameLengthMax = 255;

        protected Attachment()
            : base(null)
        {
        }

        public Attachment(string plant, string fileName)
            : base(plant)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            BlobStorageId = Guid.NewGuid();
            FileName = fileName;
        }

        public string BlobPath => string.Join('/', Plant.Substring(4), BlobStorageId.ToString(), FileName);
        public Guid BlobStorageId { get; }
        public string FileName { get; protected set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }
        public DateTime UploadedAtUtc => ModifiedAtUtc ?? CreatedAtUtc;
        public int UploadedById => ModifiedById ?? CreatedById;


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
