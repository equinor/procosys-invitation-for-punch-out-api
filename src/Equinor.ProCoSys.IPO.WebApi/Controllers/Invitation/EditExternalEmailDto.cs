﻿namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class EditExternalEmailDto
    {
        public int? Id { get; set; }
        public string Email { get; set; }
        public string RowVersion { get; set; }
    }
}