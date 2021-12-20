using System;
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
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CreateSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CreateSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
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
            => await PersonsControllerTestsHelper.UpdateSavedFilterAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                1,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                false,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task UpdateSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.UpdateSavedFilterAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                1,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                false,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldReturnBadRequest_WhenUnknownId() =>
            await PersonsControllerTestsHelper.UpdateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                9876,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                false,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "Saved filter with this ID does not exist!");

        [TestMethod]
        public async Task UpdateSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.UpdateSavedFilterAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                1,
                 Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
               false,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldReturnConflict_WhenWrongRowVersion()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            // Act
            await PersonsControllerTestsHelper.UpdateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                false,
                TestFactory.WrongButValidRowVersion,
                HttpStatusCode.Conflict);
        }
        #endregion

        #region DeleteSavedFilter
        [TestMethod]
        public async Task DeleteSavedFilter_AsAnonymous_ShouldReturnUnauthorized()
            => await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                1,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task DeleteSavedFilter_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                1,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task DeleteSavedFilter_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                1,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task DeleteSavedFilter_AsViewer_ShouldReturnConflict_WhenWrongRowVersion()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            // Act
            await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id,
                TestFactory.WrongButValidRowVersion,
                HttpStatusCode.Conflict);
        }
        #endregion
    }
}
