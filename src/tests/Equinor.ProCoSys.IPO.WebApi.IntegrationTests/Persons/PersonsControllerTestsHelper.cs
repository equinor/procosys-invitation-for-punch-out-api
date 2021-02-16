using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    public static class PersonsControllerTestsHelper
    {
        private const string Route = "Persons";

        public static async Task<int> CreateSavedFilter(
            UserType userType,
            string plant,
            string title,
            string criteria,
            bool defaultFilter,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new
            {
                projectName = KnownTestData.ProjectName,
                title,
                criteria,
                defaultFilter
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PostAsync($"{Route}/SavedFilter", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return -1;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(jsonString);
        }
    }
}
