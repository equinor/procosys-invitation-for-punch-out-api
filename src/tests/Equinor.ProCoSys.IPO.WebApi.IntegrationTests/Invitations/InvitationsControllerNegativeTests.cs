using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    [TestClass]
    public class InvitationsControllerNegativeTests : InvitationsControllerTestsBase
    {
        #region GetInvitation
        [TestMethod]
        public async Task GetInvitation_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                9999,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetInvitation_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetInvitation_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialInvitationId, 
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetInvitation_AsPlanner_ShouldReturnNotFound_WhenUnknownId()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess, 
                9999, 
                HttpStatusCode.NotFound);

        [TestMethod]
        public async Task GetInvitation_AsViewer_ShouldReturnNotFound_WhenUnknownId()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess, 
                9999, 
                HttpStatusCode.NotFound);
        #endregion

        #region Create

        [TestMethod]
        public async Task CreateInvitation_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                DateTime.Now, 
                DateTime.Today, 
                null,
                null,
                null,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CreateInvitation_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                DateTime.Now, 
                DateTime.Today, 
                null,
                null,
                null,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CreateInvitation_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                DateTime.Now,
                DateTime.Today,
                null,
                null,
                null,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CreateInvitation_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                DateTime.Now,
                DateTime.Today,
                null,
                null,
                null,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CreateInvitation_AsViewer_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                DateTime.Now,
                DateTime.Today,
                null,
                null,
                null,
                HttpStatusCode.Forbidden);

        #endregion

        #region Edit (Sign)

        #endregion
    }
}
