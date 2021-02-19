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

        #region GetSavedFiltersInProject
        [TestMethod]
        public async Task GetSavedFiltersInProject_AsAnonymous_ShouldReturnUnauthorized()
            => await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                null,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                null,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsViewer_ShouldReturnBadRequest_WhenUnknownProject() =>
            await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "12345",
                HttpStatusCode.BadRequest);

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.GetSavedFiltersInProject(
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
                "rowVersion",
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
                "rowVersion",
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
                "rowVersion",
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
                "rowVersion",
                1,
                HttpStatusCode.Forbidden);
        #endregion
    }
}
