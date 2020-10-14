using System;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class ParticipantDto
    {
        public ParticipantDto(Guid? oid, string email, string name, MeetingResponse response)
        {
            Oid = oid;
            Email = email;
            Name = name;
            Response = response;
        }

        public Guid? Oid { get; }
        public MeetingResponse Response { get; }
        public string Email { get; }
        public string Name { get; }
    }
}
