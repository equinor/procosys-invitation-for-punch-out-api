﻿using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class ParticipantDto
    {
        public ParticipantDto(int id,
            Organization organization,
            int sortKey,
            PersonDto signedBy,
            DateTime? signedAtUtc,
            string note,
            bool attended,
            bool isAttendedTouched,
            bool canSign,
            bool isSigner,
            bool canEditAttendedStatusAndNote,
            ExternalEmailDto externalEmail,
            InvitedPersonDto person,
            FunctionalRoleDto functionalRole,
            string rowVersion)
        {
            Id = id;
            Organization = organization;
            SortKey = sortKey;
            SignedBy = signedBy;
            SignedAtUtc = signedAtUtc;
            Note = note;
            Attended = attended;
            IsAttendedTouched = isAttendedTouched;
            CanSign = canSign;
            IsSigner = isSigner;
            CanEditAttendedStatusAndNote = canEditAttendedStatusAndNote;
            ExternalEmail = externalEmail;
            Person = person;
            FunctionalRole = functionalRole;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public Organization Organization { get; }
        public int SortKey { get; }
        public PersonDto SignedBy { get; }
        public DateTime? SignedAtUtc { get; }
        public string Note { get; }
        public bool Attended { get; }
        public bool IsAttendedTouched { get; }
        [Obsolete("Use isSigner")]
        public bool CanSign { get; }
        public bool IsSigner { get; }
        public bool CanEditAttendedStatusAndNote { get; }
        public ExternalEmailDto ExternalEmail { get; }
        public InvitedPersonDto Person { get; }
        public FunctionalRoleDto FunctionalRole { get; }
        public string RowVersion { get; }
    }
}
