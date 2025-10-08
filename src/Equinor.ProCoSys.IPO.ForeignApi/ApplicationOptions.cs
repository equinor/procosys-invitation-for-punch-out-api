using System;

namespace Equinor.ProCoSys.IPO.ForeignApi
{
    public record ApplicationOptions
    {
        public Guid ObjectId { get; set; }
        public string LibraryApiScope { get; set; }
    }
}
