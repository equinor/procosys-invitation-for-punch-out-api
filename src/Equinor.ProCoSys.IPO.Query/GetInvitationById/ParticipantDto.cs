using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class ParticipantDto
    {
        public ParticipantDto(
            Organization organization,
            int sortKey,
            string signedBy,
            DateTime? signedAtUtc,
            string note,
            bool attended,
            bool canSign,
            ExternalEmailDto externalEmail,
            InvitedPersonDto person,
            FunctionalRoleDto functionalRole)
        {
            Organization = organization;
            SortKey = sortKey;
            SignedBy = signedBy;
            SignedAtUtc = signedAtUtc;
            Note = note;
            Attended = attended;
            CanSign = canSign;
            ExternalEmail = externalEmail;
            Person = person;
            FunctionalRole = functionalRole;
        }

        public Organization Organization { get; }
        public int SortKey { get; }
        public string SignedBy { get; }
        public DateTime? SignedAtUtc { get; }
        public string Note { get; }
        public bool Attended { get; }
        public bool CanSign { get; }
        public ExternalEmailDto ExternalEmail { get; }
        public InvitedPersonDto Person { get; }
        public FunctionalRoleDto FunctionalRole { get; }
    }
}
