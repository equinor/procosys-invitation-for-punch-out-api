using System;
using System.Linq;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Invitation : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
    {
        public const int ProjectNameMinLength = 3;
        public const int ProjectNameMaxLength = 512;
        public const int TitleMinLength = 3;
        public const int TitleMaxLength = 250;
        public const int LocationMaxLength = 250;
        public const int DescriptionMaxLength = 4096;

        private readonly List<McPkg> _mcPkgs = new List<McPkg>();
        private readonly List<CommPkg> _commPkgs = new List<CommPkg>();
        private readonly List<Participant> _participants = new List<Participant>();
        private readonly List<Comment> _comments = new List<Comment>();
        private readonly List<Attachment> _attachments = new List<Attachment>();

        protected Invitation()
            : base(null)
        {
        }

        public Invitation(
            string plant,
            string projectName,
            string title,
            string description,
            DisciplineType type,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            string location)
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
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            Location = location;
            ObjectGuid = Guid.NewGuid();
            AddPreSaveDomainEvent(new IpoCreatedEvent(plant, ObjectGuid));
        }
        public Guid ObjectGuid { get; set; }
        public string ProjectName { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DisciplineType Type { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Location { get; set; }
        public IReadOnlyCollection<McPkg> McPkgs => _mcPkgs.AsReadOnly();
        public IReadOnlyCollection<CommPkg> CommPkgs => _commPkgs.AsReadOnly();
        public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();
        public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
        public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();
        public IpoStatus Status { get; private set; }
        public int? CompletedBy { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        public int? AcceptedBy { get; private set; }
        public DateTime? AcceptedAtUtc { get; private set; }
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
            AddPreSaveDomainEvent(new AttachmentUploadedEvent(Plant, ObjectGuid, attachment.FileName));
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
            AddPreSaveDomainEvent(new AttachmentRemovedEvent(Plant, ObjectGuid, attachment.FileName));
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

            if (Type == DisciplineType.DP)
            {
                throw new ArgumentException($"Can't add comm pkg to invitation with type DP");
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

            if (Type == DisciplineType.MDP)
            {
                throw new ArgumentException($"Can't add mc pkg to invitation with type MDP");
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

        public void CompleteIpo(Participant participant, string participantRowVersion, Person completedBy, DateTime completedAtUtc)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status != IpoStatus.Planned)
            {
                throw new Exception($"Complete on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            Status = IpoStatus.Completed;
            participant.SignedBy = completedBy.Id;
            participant.SignedAtUtc = completedAtUtc;
            participant.SetRowVersion(participantRowVersion);
            CompletedBy = completedBy.Id;
            CompletedAtUtc = completedAtUtc;
            AddPreSaveDomainEvent(new IpoCompletedEvent(Plant, ObjectGuid));
            AddPostSaveDomainEvent(new Events.PostSave.IpoCompletedEvent(Plant, ObjectGuid));
        }

        public void UnCompleteIpo(Participant participant, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status != IpoStatus.Completed)
            {
                throw new Exception($"UnComplete on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            Status = IpoStatus.Planned;
            participant.SignedBy = null;
            participant.SignedAtUtc = null;
            participant.SetRowVersion(participantRowVersion);
            CompletedAtUtc = null;
            CompletedBy = null;
            AddPreSaveDomainEvent(new IpoUnCompletedEvent(Plant, ObjectGuid));
            AddPostSaveDomainEvent(new Events.PostSave.IpoUnCompletedEvent(Plant, ObjectGuid));
        }

        public void AcceptIpo(Participant participant, string participantRowVersion, Person acceptedBy, DateTime acceptedAtUtc)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status != IpoStatus.Completed)
            {
                throw new Exception($"Accept on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            Status = IpoStatus.Accepted;
            participant.SignedBy = acceptedBy.Id;
            participant.SignedAtUtc = acceptedAtUtc;
            participant.SetRowVersion(participantRowVersion);
            AcceptedBy = acceptedBy.Id;
            AcceptedAtUtc = acceptedAtUtc;
            AddPreSaveDomainEvent(new IpoAcceptedEvent(Plant, ObjectGuid));
            AddPostSaveDomainEvent(new Events.PostSave.IpoAcceptedEvent(Plant, ObjectGuid));
        }

        public void UnAcceptIpo(Participant participant, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status != IpoStatus.Accepted)
            {
                throw new Exception($"Unaccept on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            Status = IpoStatus.Completed;
            participant.SignedBy = null;
            participant.SignedAtUtc = null;
            participant.SetRowVersion(participantRowVersion);
            AcceptedAtUtc = null;
            AcceptedBy = null;
            AddPreSaveDomainEvent(new IpoUnAcceptedEvent(Plant, ObjectGuid));
            AddPostSaveDomainEvent(new Events.PostSave.IpoUnAcceptedEvent(Plant, ObjectGuid));
        }

        public void SignIpo(Participant participant, Person signedBy, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status == IpoStatus.Canceled)
            {
                throw new Exception($"Sign on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            participant.SignedBy = signedBy.Id;
            participant.SignedAtUtc = DateTime.UtcNow;
            participant.SetRowVersion(participantRowVersion);
            AddPreSaveDomainEvent(new IpoSignedEvent(Plant, ObjectGuid));
        }

        public void EditIpo(string title, string description, DisciplineType type, DateTime startTime, DateTime endTime, string location)
        {
            if (Status != IpoStatus.Planned)
            {
                throw new Exception($"Edit on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            if (startTime > endTime)
            {
                throw new Exception($"Edit on {nameof(Invitation)} {Id} can not be performed. Start time is before end time");
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new Exception($"Edit on {nameof(Invitation)} {Id} can not be performed. Title cannot be empty");

            }

            Title = title;
            Description = description;
            Type = type;
            StartTimeUtc = startTime;
            EndTimeUtc = endTime;
            Location = location;
            AddPreSaveDomainEvent(new IpoEditedEvent(Plant, ObjectGuid));
        }

        public void AddComment(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }

            if (comment.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {comment.Plant} to item in {Plant}");
            }

            _comments.Add(comment);
            AddPreSaveDomainEvent(new CommentAddedEvent(Plant, ObjectGuid));
        }

        public void RemoveComment(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }

            if (comment.Plant != Plant)
            {
                throw new ArgumentException($"Can't remove item in {comment.Plant} from item in {Plant}");
            }

            _comments.Remove(comment);
            AddPreSaveDomainEvent(new CommentRemovedEvent(Plant, ObjectGuid));
        }

        public void CancelIpo(Person caller)
        {
            if (caller == null)
            {
                throw new ArgumentNullException(nameof(caller));
            }

            if (caller.Id != CreatedById)
            {
                throw new InvalidOperationException("Only the creator can cancel an invitation");
            }

            if (Status == IpoStatus.Canceled)
            {
                throw new Exception($"{nameof(Invitation)} {Id} is already canceled");
            }

            if (Status == IpoStatus.Accepted)
            {
                throw new Exception($"{nameof(Invitation)} {Id} is accepted");
            }

            AddPostSaveDomainEvent(new Events.PostSave.IpoCanceledEvent(Plant, ObjectGuid, Status));

            Status = IpoStatus.Canceled;
            AddPreSaveDomainEvent(new IpoCanceledEvent(Plant, ObjectGuid));
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

        public void MoveToProject(string toProject) => ProjectName = toProject;
    }
}
