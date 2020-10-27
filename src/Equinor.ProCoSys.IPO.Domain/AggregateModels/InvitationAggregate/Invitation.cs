using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Audit;
using Equinor.ProCoSys.IPO.Domain.Time;

namespace Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate
{
    public class Invitation : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
    {
        public const int ProjectNameMaxLength = 512;
        public const int TitleMaxLength = 1024;

        private readonly List<McPkg> _mcPkgs = new List<McPkg>();
        private readonly List<CommPkg> _commPkgs = new List<CommPkg>();
        private readonly List<Participant> _participants = new List<Participant>();

        private Invitation()
            : base(null)
        {
        }

        public Invitation(string plant, string projectName, string title, DisciplineType type)
            : base(plant)
        {
            ProjectName = projectName;
            Title = title;
            Type = type;
        }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public DisciplineType Type { get; set; }
        public IReadOnlyCollection<McPkg> McPkgs => _mcPkgs.AsReadOnly();
        public IReadOnlyCollection<CommPkg> CommPkgs => _commPkgs.AsReadOnly();
        public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();

        public Guid MeetingId { get; set; }

        public DateTime CreatedAtUtc { get; private set; }

        public int CreatedById { get; private set; }

        public DateTime? ModifiedAtUtc { get; private set; }

        public int? ModifiedById { get; private set; }

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
