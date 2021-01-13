using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Scope
{
    [TestClass]
    public class ScopeControllerNegativeTests : ScopeControllerTestsBase
    {
        #region GetCommPkgsInProject
        [TestMethod]
        public async Task GetCommPkgsInProject_AsAnonymous_ShouldReturnUnauthorized()
            => await ScopeControllerTestsHelper.GetCommPkgsInProjectAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                "CommPkgNo",
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetCommPkgsInProject_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await ScopeControllerTestsHelper.GetCommPkgsInProjectAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                "CommPkgNo",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetCommPkgsInProject_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await ScopeControllerTestsHelper.GetCommPkgsInProjectAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess,
                "CommPkgNo",
                HttpStatusCode.Forbidden);
        #endregion

        #region GetProjectsInPlant
        [TestMethod]
        public async Task GetProjectsInPlant_AsAnonymous_ShouldReturnUnauthorized()
            => await ScopeControllerTestsHelper.GetProjectsInPlantAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetProjectsInPlant_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await ScopeControllerTestsHelper.GetProjectsInPlantAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetProjectsInPlant_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await ScopeControllerTestsHelper.GetProjectsInPlantAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                HttpStatusCode.Forbidden);
        #endregion

        #region GetMcPkgsInProject
        [TestMethod]
        public async Task GetMcPkgsInProject_AsAnonymous_ShouldReturnUnauthorized()
            => await ScopeControllerTestsHelper.GetMcPkgsInProjectAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                "CommPkgNo",
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetMcPkgsInProject_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await ScopeControllerTestsHelper.GetMcPkgsInProjectAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                "CommPkgNo",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetMcPkgsInProject_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await ScopeControllerTestsHelper.GetMcPkgsInProjectAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess,
                "CommPkgNo",
                HttpStatusCode.Forbidden);
        #endregion
    }
}
