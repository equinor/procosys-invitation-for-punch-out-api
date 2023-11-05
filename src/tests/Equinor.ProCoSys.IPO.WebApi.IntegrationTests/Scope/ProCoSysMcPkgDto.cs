using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Scope
{
    public class ProCoSysMcPkgDto
    {
        public long Id { get; set; }
        public string McPkgNo { get; set; }
        public string Description { get; set; }
        public string DisciplineCode { get; set; }
        public string System { get; set; }
        public DateTime? RfocAcceptedAt { get; set; }
    }
}
