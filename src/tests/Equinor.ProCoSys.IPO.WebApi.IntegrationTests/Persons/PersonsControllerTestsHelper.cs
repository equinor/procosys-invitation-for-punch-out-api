using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetSavedFiltersInProject;
using Equinor.ProCoSys.IPO.WebApi.Controllers.Persons;
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

        public static async Task<List<SavedFilterDto>> GetSavedFiltersInProject(
            UserType userType,
            string plant,
            string? projectName,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var project = projectName ?? KnownTestData.ProjectName;
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync($"{Route}/SavedFilters?projectName={project}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<SavedFilterDto>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SavedFilterDto>>(jsonString);
        }

        public static async Task<string> UpdateSavedFilter(
            UserType userType,
            string plant,
            string newTitle,
            string newCriteria,
            bool defaultFilter,
            string rowVersion,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var bodyPayload = new UpdateSavedFilterDto()
            {
                Title = newTitle,
                Criteria = newCriteria,
                DefaultFilter = defaultFilter,
                RowVersion = rowVersion
            };


            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).PutAsync($"{Route}/SavedFilters/{id}", content);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return "";
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
