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
        List<AccessablePlant> AccessablePlants { get; set; }
        List<AccessableProject> AccessableProjects { get; set; }
        List<string> Permissions { get; set; }
        HttpClient HttpClient { get; set; }
    }
}
