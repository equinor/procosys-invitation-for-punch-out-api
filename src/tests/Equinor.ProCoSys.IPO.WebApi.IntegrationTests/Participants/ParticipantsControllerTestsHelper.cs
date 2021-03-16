using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Participants
{
    public static class ParticipantsControllerTestsHelper
    {
        private const string Route = "Participants";

        public static async Task<List<ProCoSysFunctionalRoleDto>> GetFunctionalRolesForIpoAsync(
            UserType userType,
            string plant,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await TestFactory.Instance.GetHttpClient(userType, plant)
                .GetAsync($"{Route}/FunctionalRoles/ByClassification/IPO");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysFunctionalRoleDto>>(content);
        }

        public static async Task<List<ProCoSysPersonDto>> GetPersonsAsync(
            UserType userType,
            string plant,
            string searchString,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection { { "searchString", searchString } };
            var url = $"{Route}/Persons{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysPersonDto>>(content);
        }

        public static async Task<List<ProCoSysPersonDto>> GetRequiredSignerPersonsAsync(
            UserType userType,
            string plant,
            string searchString,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection { { "searchString", searchString } };
            var url = $"{Route}/Persons/ByPrivileges/RequiredSigners{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysPersonDto>>(content);
        }

        public static async Task<List<ProCoSysPersonDto>> GetAdditionalSignerPersonsAsync(
            UserType userType,
            string plant,
            string searchString,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection { { "searchString", searchString } };
            var url = $"{Route}/Persons/ByPrivileges/AdditionalSigners{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysPersonDto>>(content);
        }

        public static async Task<List<ProCoSysPersonDto>> GetSignerPersonsAsync(
            UserType userType,
            string plant,
            string searchString,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var parameters = new ParameterCollection { { "searchString", searchString } };
            var url = $"{Route}/Persons/ByPrivileges/Signers{parameters}";
            var response = await TestFactory.Instance.GetHttpClient(userType, plant).GetAsync(url);

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProCoSysPersonDto>>(content);
        }
    }
}
