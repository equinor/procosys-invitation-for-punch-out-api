using System;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditInvitedPersonDto
    {
        private int? _id;

        public int? Id
        {
            // treat zero as no value. .NET framework seem to initiate nullable int with 0 when not given from client 
            get => _id.HasValue && _id.Value != 0 ? _id.Value : null;
            set => _id = value;
        }
        public Guid? AzureOid { get; set; }
        public string Email { get; set; }
        public bool Required { get; set; }
        public string RowVersion { get; set; }
    }
}
