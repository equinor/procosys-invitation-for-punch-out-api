using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.SendToFam;

[TestClass]
public class SendToFamTests
{
    private const string WrongApiKey = "Wrong api key";

    [TestMethod]
    public async Task SendToFam_AsAnonymous_WrongApiKey_ShouldReturnUnauthorized()
        => await SendToFamTestsHelper.SendToFamAsync(
            UserType.Anonymous,
            WrongApiKey,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task SendToFam_AsAnonymous_With_CorrectApi_Key_ShouldReturnUnauthorized()
        => await SendToFamTestsHelper.SendToFamAsync(
            UserType.Anonymous,
            TestFactory.SendToFamCorrectApiKey,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task SendToFam_AsPlanner_Without_ApiKey_ShouldReturnUnauthorized()
        => await SendToFamTestsHelper.SendToFamAsync(
            UserType.Planner,
            string.Empty,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task SendToFam_AsPlanner_With_Wrong_ApiKey_ShouldReturnUnauthorized() =>
        await SendToFamTestsHelper.SendToFamAsync(
            UserType.Planner,
            WrongApiKey,
            HttpStatusCode.Forbidden);

    [TestMethod]
    public async Task SendToFam_AsPlanner_With_Correct_ApiKey_ShouldReturn_OK() =>
        await SendToFamTestsHelper.SendToFamAsync(
            UserType.Planner,
            TestFactory.SendToFamCorrectApiKey,
            HttpStatusCode.OK);
}
