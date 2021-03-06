﻿using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class EditInvitationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ProjectName { get; set; }
        public DisciplineType Type { get; set; }
        public IEnumerable<ParticipantDtoEdit> UpdatedParticipants { get; set; }
        public IEnumerable<string> UpdatedMcPkgScope { get; set; }
        public IEnumerable<string> UpdatedCommPkgScope { get; set; }
        public string RowVersion { get; set; }
    }
}
