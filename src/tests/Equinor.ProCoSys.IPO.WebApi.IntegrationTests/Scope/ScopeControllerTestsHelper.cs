using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Scope
{
    public static class ScopeControllerTestsHelper
    {
        public static async Task<List<ProCoSysCommPkgDto>> GetCommPkgsInProjectAsync(
            UserType userType,
            string plant,
            string projectName,
            string startsWithCommPkgNo,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection
            {
                { "projectName", projectName },
                { "startsWithCommPkgNo", startsWithCommPkgNo }
            };
            var url = $"/CommPkgs{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysCommPkgDto>>(content);
        }

        public static async Task<List<ProCoSysProjectDto>> GetProjectsInPlantAsync(
            UserType userType,
            string plant,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .GetAsync("/Projects");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysProjectDto>>(content);
        }

        public static async Task<List<ProCoSysMcPkgDto>> GetMcPkgsInProjectAsync(
            UserType userType,
            string plant,
            string projectName,
            string commPkgNo,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection
            {
                { "projectName", projectName },
                { "commPkgNo", commPkgNo }
            };
            var url = $"/McPkgs{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysMcPkgDto>>(content);
        }
    }
}
