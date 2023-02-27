using System.Diagnostics;

namespace Equinor.ProCoSys.Auth.Permission
{
    [DebuggerDisplay("{Name} {Id} {HasAccess}")]
    public class AccessableProject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasAccess { get; set; }
    }
}
