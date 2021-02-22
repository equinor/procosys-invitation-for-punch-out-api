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

        #region GetSavedFiltersInProject
        [TestMethod]
        public async Task GetSavedFiltersInProject_AsAnonymous_ShouldReturnUnauthorized()
            => await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                null,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                null,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                null,
                HttpStatusCode.Forbidden);
        #endregion

        #region UpdateSavedFilter
        [TestMethod]
        public async Task UpdateSavedFilter_AsAnonymous_ShouldReturnUnauthorized()
            => await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "new title",
                "new criteria",
                false,
                "cm93VmVyc2lvbg==",
                1,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task UpdateSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                "new title",
                "new criteria",
                false,
                "cm93VmVyc2lvbg==",
                1,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldReturnBadRequest_WhenUnknownId() =>
            await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "new title",
                "new criteria",
                false,
                "cm93VmVyc2lvbg==",
                9999,
                HttpStatusCode.BadRequest);

        [TestMethod]
        public async Task UpdateSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                "new title",
                "new criteria",
                false,
                "cm93VmVyc2lvbg==",
                1,
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
