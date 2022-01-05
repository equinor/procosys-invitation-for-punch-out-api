using System;
using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class InvitedPersonDto
    {
        public InvitedPersonDto(
            int id,
            string firstName,
            string lastName,
            string userName,
            Guid azureOid,
            string email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            AzureOid = azureOid;
            Email = email;
        }

        public OutlookResponse? Response { get; set; }
        [Obsolete("Use parent Participant.Id")]
        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string UserName { get; }
        public Guid AzureOid { get; }
        public string Email { get; }
        public bool Required { get; set; }
    }
}
