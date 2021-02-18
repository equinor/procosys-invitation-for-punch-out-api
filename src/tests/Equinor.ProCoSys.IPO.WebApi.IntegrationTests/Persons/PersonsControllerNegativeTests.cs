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
            => await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "title",
                "criteria",
                true,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CreateSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                "title",
                "criteria",
                true,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CreateSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                "title",
                "criteria",
                true,
                HttpStatusCode.Forbidden);
        #endregion
    }
}
