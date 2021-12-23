using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Me
{
    public static class MeControllerTestsHelper
    {
        private const string Route = "Me";

        public static async Task<OutstandingIposResultDto> GetOutstandingIposAsync(
            UserType userType,
            string plant,
            string projectName,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var project = projectName ?? KnownTestData.ProjectName;
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/OutstandingIpos?projectName={project}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OutstandingIposResultDto>(jsonString);
        }
    }
}
