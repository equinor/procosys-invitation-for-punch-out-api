﻿using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class FunctionalRoleDto
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public IEnumerable<PersonDto> Persons { get; set; }
    }
}
