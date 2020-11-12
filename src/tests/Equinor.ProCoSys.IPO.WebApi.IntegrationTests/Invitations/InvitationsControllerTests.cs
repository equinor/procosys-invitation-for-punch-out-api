using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    [TestClass]
    public class InvitationsControllerTests : InvitationsControllerTestsBase
    {
        [TestMethod]
        public async Task GetInvitation_AsViewer_ShouldGetInvitation()
        {
            // Act
            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                ViewerClient(TestFactory.PlantWithAccess), 
                InitialInvitationId);

            // Assert
            Assert.IsNotNull(invitation);
            Assert.IsNotNull(invitation.RowVersion);
        }

        //[TestMethod]
        //public async Task CreateInvitation_AsPlanner_ShouldCreateInvitation()
        //{
            // Act
            //var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
            //    PlannerClient(TestFactory.PlantWithAccess),
            //    Guid.NewGuid().ToString(),
            //    Guid.NewGuid().ToString(),
            //    Guid.NewGuid().ToString(),
            //    DisciplineType.DP);

            //// Assert
            //Assert.IsTrue(id > 0);
        //}
    }
}
