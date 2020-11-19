using System.Collections.Generic;
using System.Net.Http;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public interface ITestUser
    {
        TestProfile Profile { get; set; }
        List<ProCoSysPlant> ProCoSysPlants { get; set; }
        List<ProCoSysProject> ProCoSysProjects { get; set; }
        List<string> ProCoSysPermissions { get; set; }
        HttpClient HttpClient { get; set; }
    }
}
