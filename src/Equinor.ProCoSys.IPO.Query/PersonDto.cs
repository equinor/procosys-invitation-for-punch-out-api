using System;

namespace Equinor.ProCoSys.IPO.Query
{
    public class PersonDto
    {
        public PersonDto(
            int id,
            string firstName,
            string lastName,
            string userName,
            Guid azureOid,
            string email,
            string rowVersion)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            AzureOid = azureOid;
            Email = email;
            RowVersion = rowVersion;
        }

        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string UserName { get; }
        public Guid AzureOid { get; }
        public string Email { get; }
        public string RowVersion { get; }
    }
}
