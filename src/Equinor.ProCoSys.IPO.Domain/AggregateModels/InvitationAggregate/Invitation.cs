using System;
using System.Linq;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Invitation : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid
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
            Project project,
            string title,
            string description,
            DisciplineType type,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            string location,
            List<McPkg> mcPkgs,
            List<CommPkg> commPkgs)
            : base(plant)
        {
            mcPkgs ??= new List<McPkg>();
            commPkgs ??= new List<CommPkg>();

            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (project.Plant != plant)
            {
                throw new ArgumentException($"Can't relate {nameof(project)} in {project.Plant} to item in {plant}");
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            ProjectId = project.Id;
            Title = title;
            Description = description;

            SetScope(type, mcPkgs, commPkgs);

            Status = IpoStatus.Planned;
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
            Location = location;
            Guid = Guid.NewGuid();
            ObjectGuid = Guid;
            AddDomainEvent(new IpoCreatedEvent(plant, Guid));
        }

        // private setters needed for Entity Framework
        public Guid Guid { get; private set; }
        [Obsolete("Keep for migration only. To be removed in next version")]
        public Guid ObjectGuid { get; private set; }
        public int ProjectId { get; private set; }
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
            AddDomainEvent(new AttachmentUploadedEvent(Plant, Guid, attachment.FileName));
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
            AddDomainEvent(new AttachmentRemovedEvent(Plant, Guid, attachment.FileName));
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

        public void UpdateNote(
            Participant participant,
            string note,
            string participantRowVersion)
        {
            if (Status == IpoStatus.Canceled)
            {
                throw new Exception($"Update on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }
            participant.Note = note;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new NoteUpdatedEvent(Plant, Guid, note));
        }

        public void UpdateAttendedStatus(
            Participant participant,
            bool attended,
            string participantRowVersion)
        {
            if (Status == IpoStatus.Canceled)
            {
                throw new Exception($"Update on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }
            participant.Attended = attended;
            participant.IsAttendedTouched = true;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new AttendedStatusUpdatedEvent(Plant, Guid));
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
            AddDomainEvent(new IpoCompletedEvent(Plant, Guid, participant));

            AddPostSaveDomainEvent(new Events.PostSave.IpoCompletedEvent(Plant, Guid));
        }

        public List<string> GetCompleterEmails()
            => this.Participants.Where(
                    p => p.Organization == Organization.ConstructionCompany && p.SortKey == 1 && p.Email != null)
                .Select(p => p.Email).ToList();


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
            AddDomainEvent(new IpoUnCompletedEvent(Plant, Guid, participant));
            AddPostSaveDomainEvent(new Events.PostSave.IpoUnCompletedEvent(Plant, Guid));
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
            AddDomainEvent(new IpoAcceptedEvent(Plant, Guid, participant));
            AddPostSaveDomainEvent(new Events.PostSave.IpoAcceptedEvent(Plant, Guid));
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
            AddDomainEvent(new IpoUnAcceptedEvent(Plant, Guid, participant));
            AddPostSaveDomainEvent(new Events.PostSave.IpoUnAcceptedEvent(Plant, Guid));
        }

        public void SignIpo(Participant participant, Person signedBy, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status is IpoStatus.Canceled or IpoStatus.ScopeHandedOver)
            {
                throw new Exception($"Sign on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            participant.SignedBy = signedBy.Id;
            participant.SignedAtUtc = DateTime.UtcNow;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new IpoSignedEvent(Plant, Guid, participant, signedBy));
        }

        public void UnSignIpo(Participant participant, Person unSignedBy, string participantRowVersion)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (Status == IpoStatus.Canceled)
            {
                throw new Exception($"Unsign on {nameof(Invitation)} {Id} can not be performed. Status = {Status}");
            }

            participant.SignedBy = null;
            participant.SignedAtUtc = null;
            participant.SetRowVersion(participantRowVersion);
            AddDomainEvent(new IpoUnSignedEvent(Plant, Guid, participant, unSignedBy));
        }

        public void EditIpo(
            string title,
            string description,
            DisciplineType type,
            DateTime startTime,
            DateTime endTime,
            string location,
            IList<McPkg> mcPkgScope,
            IList<CommPkg> commPkgScope)
        {
            mcPkgScope ??= new List<McPkg>();
            commPkgScope ??= new List<CommPkg>();
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

            SetScope(type, mcPkgScope, commPkgScope);

            StartTimeUtc = startTime;
            EndTimeUtc = endTime;
            Location = location;
            AddDomainEvent(new IpoEditedEvent(Plant, Guid));
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
            AddDomainEvent(new CommentAddedEvent(Plant, Guid));
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
            AddDomainEvent(new CommentRemovedEvent(Plant, Guid));
        }

        public void CancelIpo(Person caller)
        {
            if (caller == null)
            {
                throw new ArgumentNullException(nameof(caller));
            }

            if (Status == IpoStatus.Canceled)
            {
                throw new Exception($"{nameof(Invitation)} {Id} is already canceled");
            }

            if (Status == IpoStatus.Accepted)
            {
                throw new Exception($"{nameof(Invitation)} {Id} is accepted");
            }

            AddPostSaveDomainEvent(new Events.PostSave.IpoCanceledEvent(Plant, Guid, Status));
            Status = IpoStatus.Canceled;
            AddDomainEvent(new IpoCanceledEvent(Plant, Guid));
        }

        public void ScopeHandedOver()
        {
            if (Status is IpoStatus.Canceled or IpoStatus.ScopeHandedOver)
            {
                throw new Exception($"{nameof(Invitation)} {Id} is {Status}. Cannot set status to {IpoStatus.ScopeHandedOver}");
            }

            Status = IpoStatus.ScopeHandedOver;
            AddDomainEvent(new ScopeHandedOverEvent(Plant, Guid));
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

        public void MoveToProject(Project toProject)
        {
            if (toProject is null)
            {
                throw new ArgumentNullException(nameof(toProject));
            }

            ProjectId = toProject.Id;
        }

        private void AddCommPkg(CommPkg commPkg)
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

            if (_commPkgs.Any(c => c.CommPkgNo == commPkg.CommPkgNo))
            {
                return;
            }
            _commPkgs.Add(commPkg);
        }

        private void AddMcPkg(McPkg mcPkg)
        {
            if (mcPkg == null)
            {
                throw new ArgumentNullException(nameof(mcPkg));
            }

            if (mcPkg.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {mcPkg.Plant} to item in {Plant}");
            }

            if (Type != DisciplineType.DP)
            {
                throw new ArgumentException($"Can't add mc pkg to invitation with type {Type}");
            }

            if (_mcPkgs.Any(m => m.McPkgNo == mcPkg.McPkgNo))
            {
                return;
            }
            _mcPkgs.Add(mcPkg);
        }

        private void SetDpScope(IList<McPkg> mcPkgs)
        {
            foreach (var mcPkg in mcPkgs)
            {
                AddMcPkg(mcPkg);
            }

            RemoveOldMcPkgs(mcPkgs);
        }

        private void RemoveOldMcPkgs(IList<McPkg> mcPkgs)
            => _mcPkgs.RemoveAll(x => !mcPkgs.Select(y => y.McPkgNo).Contains(x.McPkgNo));

        private void SetMdpScope(IList<CommPkg> commPkgs)
        {
            foreach (var commPkg in commPkgs)
            {
                AddCommPkg(commPkg);
            }

            RemoveOldCommPkgs(commPkgs);
        }

        private void RemoveOldCommPkgs(IList<CommPkg> commPkgs)
            => _commPkgs.RemoveAll(x => !commPkgs.Select(y => y.CommPkgNo).Contains(x.CommPkgNo));

        private void ClearDpScope() => _mcPkgs.Clear();

        private void ClearMdpScope() => _commPkgs.Clear();

        private void SetScope(DisciplineType type, IList<McPkg> mcPkgs, IList<CommPkg> commPkgs)
        {
            Type = type;

            switch (type)
            {
                case DisciplineType.DP when !mcPkgs.Any() || commPkgs.Any():
                    throw new ArgumentException("DP must have mc pkg scope and mc pkg scope only");
                case DisciplineType.MDP when mcPkgs.Any() || !commPkgs.Any():
                    throw new ArgumentException("MDP must have comm pkg scope and comm pkg scope only");
                case DisciplineType.DP:
                    SetDpScope(mcPkgs);
                    ClearMdpScope();
                    break;
                case DisciplineType.MDP:
                    SetMdpScope(commPkgs);
                    ClearDpScope();
                    break;
            }
        }
    }
}
