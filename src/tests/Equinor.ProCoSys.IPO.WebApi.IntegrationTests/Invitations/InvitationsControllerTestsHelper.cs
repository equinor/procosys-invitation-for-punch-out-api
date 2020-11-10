using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public static class InvitationsControllerTestsHelper
    {
        private const string _route = "Invitations";
        
        public static async Task<InvitationDetailsDto> GetInvitationAsync(
            HttpClient client,
            int id,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            var response = await client.GetAsync($"{_route}/{id}");

            await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<InvitationDetailsDto>(content);
        }

        public static async Task<int> CreateInvitationAsync(
            HttpClient client,
            string title,
            string description,
            string location,
            DisciplineType type,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            string expectedMessageOnBadRequest = null)
        {
            // todo Add correct payload to be able to create an invitation (i.e run a positive test)
            var bodyPayload = new
            {
                projectName = SeedingData.ProjectName,
                title,
                description,
                location,
                type
            };

            var serializePayload = JsonConvert.SerializeObject(bodyPayload);
            var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_route, content);

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
