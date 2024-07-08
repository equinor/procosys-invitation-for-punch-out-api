using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Fam;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.SendToFam;

public class SendToFamTestsHelper
{
    private const string Route = "FamSender";

    public static async Task<string> SendToFamAsync(
        UserType userType,
        string sendToFamApiKey,
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
        string expectedMessageOnBadRequest = null,
        string expectedMessageOnInternalServerError = null)
    {
        TestFactory.Instance
            .FamOptionsMock
            .Setup(x => x.CurrentValue)
            .Returns(new FamOptions { SendToFamApiKey = TestFactory.SendToFamCorrectApiKey });

        var bodyPayload = new { };

        var serializePayload = JsonConvert.SerializeObject(bodyPayload);
        var content = new StringContent(serializePayload, Encoding.UTF8, "application/json");
        var httpClient = TestFactory.Instance.GetHttpClient(userType, TestFactory.PlantWithAccess);
        AddApiKeyToHeader(httpClient, sendToFamApiKey);
        var response = await httpClient.PostAsync($"{Route}/SendAllData", content);

        await TestsHelper.AssertResponseAsync(response, expectedStatusCode, expectedMessageOnBadRequest);
        await TestsHelper.AssertInternalServerErrorAsync(response, expectedStatusCode, expectedMessageOnInternalServerError);

        return await response.Content.ReadAsStringAsync();
    }

    private static void AddApiKeyToHeader(HttpClient client, string sendToFamApiKey)
    {
        var apiKeyName = ActionFilters.SendToFamApiKeyAttribute.FamApiKeyHeader;

        if (client.DefaultRequestHeaders.Contains(apiKeyName))
        {
            client.DefaultRequestHeaders.Remove(apiKeyName);
        }

        client.DefaultRequestHeaders.Add(apiKeyName, sendToFamApiKey);

    }
}
