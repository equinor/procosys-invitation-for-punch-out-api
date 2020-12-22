using System;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class TestProfile
    {
        public string Oid { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool IsAppToken { get; set; } = false;

        public PersonForCommand AsPersonForCommand(bool required) 
            => new PersonForCommand(Guid.Parse(Oid), FirstName, LastName, Email, required);

        public ProCoSysPerson AsProCoSysPerson() =>
            new ProCoSysPerson
            {
                AzureOid = Oid,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName
            };

        public override string ToString() => $"{FullName} {Oid}";
    }
}
