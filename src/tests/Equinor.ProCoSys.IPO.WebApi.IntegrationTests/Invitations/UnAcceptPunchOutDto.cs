using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class UnAcceptPunchOutDto
    {
        public Guid ObjectGuid { get; set; }
        public string InvitationRowVersion { get; set; }
        public string ParticipantRowVersion { get; set; }
    }
}
