using System.Collections.Generic;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditFunctionalRoleDto
    {
        private int? _id;

        public int? Id
        {
            // treat zero as no value. .NET framework seem to initiate nullable int with 0 when not given from client 
            get => _id.HasValue && _id.Value != 0 ? _id.Value : null;
            set => _id = value;
        }
        public string Code { get; set; }
        public IEnumerable<EditInvitedPersonDto> Persons { get; set; }
        public string RowVersion { get; set; }
    }
}
