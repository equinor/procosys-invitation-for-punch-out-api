using System;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
                AnonymousClient(TestFactory.UnknownPlant),
                9999,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetInvitation_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                AuthenticatedHackerClient(TestFactory.UnknownPlant),
                9999,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetInvitation_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                AuthenticatedHackerClient(TestFactory.PlantWithAccess),
                InitialInvitationId, 
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetInvitation_AsPlanner_ShouldReturnNotFound_WhenUnknownId()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                PlannerClient(TestFactory.PlantWithAccess), 
                9999, 
                HttpStatusCode.NotFound);

        [TestMethod]
        public async Task GetInvitation_AsViewer_ShouldReturnNotFound_WhenUnknownId()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                ViewerClient(TestFactory.PlantWithAccess), 
                9999, 
                HttpStatusCode.NotFound);
        #endregion

        #region Create

        [TestMethod]
        public async Task CreateInvitation_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                AnonymousClient(TestFactory.UnknownPlant),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CreateInvitation_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                AuthenticatedHackerClient(TestFactory.UnknownPlant),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CreateInvitation_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                AuthenticatedHackerClient(TestFactory.PlantWithAccess),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CreateInvitation_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                SignerClient(TestFactory.PlantWithAccess),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CreateInvitation_AsViewer_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                ViewerClient(TestFactory.PlantWithAccess),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                HttpStatusCode.Forbidden);

        #endregion

        #region Edit (Sign)

        #endregion
    }
}
