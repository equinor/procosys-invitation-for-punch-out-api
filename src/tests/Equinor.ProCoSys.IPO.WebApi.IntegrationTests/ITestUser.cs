﻿using System.Collections.Generic;
using System.Net.Http;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Person;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public interface ITestUser
    {
        TestProfile Profile { get; set; }
        ProCoSysPerson AuthProCoSysPerson { get; }
        List<ProCoSysPlant> ProCoSysPlants { get; set; }
        List<ProCoSysProject> ProCoSysProjects { get; set; }
        List<string> ProCoSysPermissions { get; set; }
        HttpClient HttpClient { get; set; }
    }
}
