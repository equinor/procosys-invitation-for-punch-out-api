using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole
{
    public class ProCoSysFunctionalRole
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string InformationEmail { get; set; }
        public bool? UsePersonalEmail { get; set; }
        public IEnumerable<Person> Persons { get; set; }
    }
}
