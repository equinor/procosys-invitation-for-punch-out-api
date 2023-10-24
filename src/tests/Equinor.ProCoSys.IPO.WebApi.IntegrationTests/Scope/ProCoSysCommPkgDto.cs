﻿using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Scope
{
    public class ProCoSysCommPkgDto
    {
        public long Id { get; set; }
        public string CommPkgNo { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string System { get; set; }
        public DateTime? RfocAcceptedAt { get; set; }
    }
}
