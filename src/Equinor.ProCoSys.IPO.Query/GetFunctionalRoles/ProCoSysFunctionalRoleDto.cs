using System.Collections.Generic;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;

namespace Equinor.ProCoSys.IPO.Query.GetFunctionalRoles
{
    public class ProCoSysFunctionalRoleDto
    {
        public ProCoSysFunctionalRoleDto(
            string code,
            string description,
            string email,
            string informationEmail,
            bool? usePersonalEmail,
            IEnumerable<Person> persons)
        {
            Code = code;
            Description = description;
            Email = email;
            InformationEmail = informationEmail;
            UsePersonalEmail = usePersonalEmail;
            Persons = persons;
        }

        public string Code { get; }
        public string Description { get; }
        public string Email { get; }
        public string InformationEmail { get; }
        public bool? UsePersonalEmail { get; }
        public IEnumerable<Person> Persons { get; }
    }
}
