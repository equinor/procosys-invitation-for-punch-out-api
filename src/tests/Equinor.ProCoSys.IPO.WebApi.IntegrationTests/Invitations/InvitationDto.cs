﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class InvitationDto
    {
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DisciplineType Type { get; set; }
        public IpoStatus Status { get; set; }
        public int? CompletedBy { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public PersonDto CreatedBy { get; set; }
        public string RowVersion { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public IEnumerable<ParticipantDto> Participants { get; set; }
        public IEnumerable<McPkgScopeDto> McPkgScope { get; set; }
        public IEnumerable<CommPkgScopeDto> CommPkgScope { get; set; }
    }
}
