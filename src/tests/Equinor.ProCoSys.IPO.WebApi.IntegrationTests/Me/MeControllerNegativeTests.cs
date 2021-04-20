using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Me
{
    [TestClass]
    public class MeControllerNegativeTests : MeControllerTestsBase
    {
        #region GetOutstandingIpos
        [TestMethod]
        public async Task GetOutstandingIpos_AsAnonymous_ShouldReturnUnauthorized()
            => await MeControllerTestsHelper.GetOutstandingIposAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                null,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetOutstandingIpos_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await MeControllerTestsHelper.GetOutstandingIposAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                null,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetOutstandingIpos_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await MeControllerTestsHelper.GetOutstandingIposAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                null,
                HttpStatusCode.Forbidden);
        #endregion
    }
}
