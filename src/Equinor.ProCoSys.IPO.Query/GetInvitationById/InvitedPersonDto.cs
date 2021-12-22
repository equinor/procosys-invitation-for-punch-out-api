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
            string email,
            string rowVersion)
        {
            Person = new PersonDto(id, firstName, lastName, userName, azureOid, email, rowVersion);
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            AzureOid = azureOid;
            Email = email;
            RowVersion = rowVersion;
        }

        [Obsolete]
        public PersonDto Person { get; }
        public OutlookResponse? Response { get; set; }
        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string UserName { get; }
        public Guid AzureOid { get; }
        public string Email { get; }
        public bool Required { get; set; }
        public string RowVersion { get; }
    }
}
