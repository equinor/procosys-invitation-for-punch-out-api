using System;
using System.Linq;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.Procosys.IPO.Domain.Events;
using Equinor.ProCoSys.IPO.Domain.Events;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Invitation : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
    {
        public const int ProjectNameMaxLength = 512;
        public const int TitleMaxLength = 1024;
        public const int DescriptionMaxLength = 4096;

        private readonly List<McPkg> _mcPkgs = new List<McPkg>();
        private readonly List<CommPkg> _commPkgs = new List<CommPkg>();
        private readonly List<Participant> _participants = new List<Participant>();
        private readonly List<Attachment> _attachments = new List<Attachment>();

        protected Invitation()
            : base(null)
        {
        }

        public Invitation(string plant, string projectName, string title, string description, DisciplineType type)
            : base(plant)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentNullException(nameof(projectName));
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            ProjectName = projectName;
            Title = title;
            Description = description;
            Type = type;
            Status = IpoStatus.Planned;
            ObjectGuid = Guid.NewGuid();
            AddDomainEvent(new IpoCreatedEvent(plant, ObjectGuid));
        }
        public Guid ObjectGuid { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DisciplineType Type { get; set; }
        public IReadOnlyCollection<McPkg> McPkgs => _mcPkgs.AsReadOnly();
        public IReadOnlyCollection<CommPkg> CommPkgs => _commPkgs.AsReadOnly();
        public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();
        public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();
        public IpoStatus Status { get; set; }
        public Guid MeetingId { get; set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }

        public void AddAttachment(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (attachment.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {attachment.Plant} to item in {Plant}");
            }

            _attachments.Add(attachment);
            AddDomainEvent(new AttachmentUploadedEvent(Plant, ObjectGuid, attachment.FileName));
        }

        public void RemoveAttachment(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (attachment.Plant != Plant)
            {
                throw new ArgumentException($"Can't remove item in {attachment.Plant} from item in {Plant}");
            }

            _attachments.Remove(attachment);
            AddDomainEvent(new AttachmentRemovedEvent(Plant, ObjectGuid, attachment.FileName));
        }

        public void AddCommPkg(CommPkg commPkg)
        {
            if (commPkg == null)
            {
                throw new ArgumentNullException(nameof(commPkg));
            }

            if (commPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {commPkg.Plant} to item in {Plant}");
            }

            _commPkgs.Add(commPkg);
        }

        public void RemoveCommPkg(CommPkg commPkg)
        {
            if (commPkg == null)
            {
                throw new ArgumentNullException(nameof(commPkg));
            }

            if (commPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't remove item in {commPkg.Plant} from item in {Plant}");
            }

            _commPkgs.Remove(commPkg);
        }

        public void AddMcPkg(McPkg mcPkg)
        {
            if (mcPkg == null)
            {
                throw new ArgumentNullException(nameof(mcPkg));
            }

            if (mcPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {mcPkg.Plant} to item in {Plant}");
            }

            _mcPkgs.Add(mcPkg);
        }

        public void RemoveMcPkg(McPkg mcPkg)
        {
            if (mcPkg == null)
            {
                throw new ArgumentNullException(nameof(mcPkg));
            }

            if (mcPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't remove item in {mcPkg.Plant} from item in {Plant}");
            }

            _mcPkgs.Remove(mcPkg);
        }

        public void AddParticipant(Participant participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (participant.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {participant.Plant} to item in {Plant}");
            }

            _participants.Add(participant);
        }

        public void RemoveParticipant(Participant participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (participant.Plant != Plant)
            {
                throw new ArgumentException($"Can't remove item in {participant.Plant} from item in {Plant}");
            }

            _participants.Remove(participant);
        }

        public void UpdateParticipant(
            int participantId,
            Organization organization,
            IpoParticipantType type,
            string functionalRoleCode,
            string firstName,
            string lastName,
            string email,
            Guid? azureOid,
            int sortKey,
            string participantRowVersion)
        {
            var participant = Participants.Single(p => p.Id == participantId);
            participant.Organization = organization;
            participant.Type = type;
            participant.FunctionalRoleCode = functionalRoleCode;
            participant.FirstName = firstName;
            participant.LastName = lastName;
            participant.Email = email;
            participant.AzureOid = azureOid;
            participant.SortKey = sortKey;
            participant.SetRowVersion(participantRowVersion);
        }

        public void CompleteIpo(Participant participant, string signedBy, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }
            Status = IpoStatus.Completed;
            participant.SignedBy = signedBy;
            participant.SignedAtUtc = DateTime.UtcNow;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new IpoCompletedEvent(Plant, ObjectGuid));
        }

        public void AcceptIpo(Participant participant, string signedBy, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }
            Status = IpoStatus.Accepted;
            participant.SignedBy = signedBy;
            participant.SignedAtUtc = DateTime.UtcNow;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new IpoAcceptedEvent(Plant, ObjectGuid));
        }

        public void SignIpo(Participant participant, string signedBy, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }
            participant.SignedBy = signedBy;
            participant.SignedAtUtc = DateTime.UtcNow;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new IpoSignedEvent(Plant, ObjectGuid));
        }

        public void EditIpo(string title, string description, DisciplineType type)
        {
            Title = title;
            Description = description;
            Type = type;
            AddDomainEvent(new IpoEditedEvent(Plant, ObjectGuid));
        }

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
