using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    [TestClass]
    public class PersonsControllerNegativeTests : PersonsControllerTestsBase
    {
        #region CreateSavedFilter
        [TestMethod]
        public async Task CreateSavedFilter_AsAnonymous_ShouldReturnUnauthorized()
            => await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "title",
                "criteria",
                true,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CreateSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                "title",
                "criteria",
                true,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CreateSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                "title",
                "criteria",
                true,
                HttpStatusCode.Forbidden);
        #endregion

        #region DeleteSavedFilter
        [TestMethod]
        public async Task DeleteSavedFilter_AsAnonymous_ShouldReturnUnauthorized()
            => await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                1,
                "AAAAAAAAABA=",
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task DeleteSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                1,
                "AAAAAAAAABA=",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task DeleteSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                1,
                "AAAAAAAAABA=",
                HttpStatusCode.Forbidden);
        #endregion
    }
}
