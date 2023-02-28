using System.Diagnostics;

namespace Equinor.ProCoSys.Auth.Person
{
    [DebuggerDisplay("{FirstName} {LastName} {AzureOid}")]
    public class ProCoSysPerson
    {
        public string AzureOid { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
