using System;
using System.Globalization;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetOutstandingIpos
{
    public class InvitationProjectionDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public IpoStatus Status { get; set; }

        public int ParticipantId { get; set; }

        public Guid? AzureOid { get; set; }

        public string FunctionalRoleCode { get; set; }

        public Organization Organization { get; set; }

        public DateTime? SignedAtUtc { get; set; }

        public int SortKey { get; set; }
        
        public IpoParticipantType Type { get; set; }
        public int? SignedBy { get; set; }

      public InvitationProjectionDto(int id, string description, IpoStatus status, int participantId, Guid azureOid, string functionalRoleCode, Organization organization, DateTime? signedAtUtc, int sortKey, IpoParticipantType type, int? signedBy)
        {
            Id = id;
            Description = description;
            Status = status;
            ParticipantId = participantId;
            AzureOid = azureOid;
            FunctionalRoleCode = functionalRoleCode;
            Organization = organization;
            SignedAtUtc = signedAtUtc;
            SortKey = sortKey;
            Type = type;
            SignedBy = signedBy;
        }

      public InvitationProjectionDto(Invitation i, Participant p)
      {
          Id = i.Id;
          Description = i.Description;
            Status = i.Status;
            ParticipantId = ParticipantId = p.Id;
            AzureOid = p.AzureOid;
            FunctionalRoleCode = p.FunctionalRoleCode;
            Organization = p.Organization;
            SignedAtUtc = p.SignedAtUtc;
            SortKey = p.SortKey;
            Type = p.Type;
            SignedBy = p.SignedBy;
        }
    }
}
