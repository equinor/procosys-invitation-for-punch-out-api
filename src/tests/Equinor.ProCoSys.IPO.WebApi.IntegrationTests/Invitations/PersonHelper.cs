using System;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.ForeignApi;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class PersonHelper
    {
        public PersonHelper(
            string azureOid,
            string firstname,
            string lastName,
            string userName,
            string email,
            int id,
            string rowVersion)
        {
            AzureOid = azureOid;
            FirstName = firstname;
            LastName = lastName;
            UserName = userName;
            Email = email;
            Id = id;
            RowVersion = rowVersion;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AzureOid { get; set; }
        public string RowVersion { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }

        public PersonForCommand AsPersonForCommand(bool required) 
            => new PersonForCommand(Guid.Parse(AzureOid), Email, required);

        public ProCoSysPerson AsProCoSysPerson() =>
            new ProCoSysPerson
            {
                AzureOid = AzureOid,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName
            };
    }
}
