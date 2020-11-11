using System.Collections.Generic;
using System.Net.Http;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class TestUser : ITestUser
    {
        public TestProfile Profile { get; set; }
        public List<ProCoSysPlant> ProCoSysPlants { get; set; }
        public List<ProCoSysProject> ProCoSysProjects { get; set; }
        public List<string> ProCoSysPermissions { get; set; }
        public HttpClient HttpClient { get; set; }

        public override string ToString() => Profile?.ToString();
    }
}
