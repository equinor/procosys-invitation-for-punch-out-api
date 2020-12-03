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

        [TestMethod]
        public async Task GetInvitationsByCommPkgNo_AsViewer_ShouldGetInvitations()
        {
            // Act
            var invitations = await InvitationsControllerTestsHelper.GetInvitationsByCommPkgNoAsync(
                ViewerClient(TestFactory.PlantWithAccess),
                KnownTestData.CommPkgNo,
                TestFactory.ProjectWithAccess);

            // Assert
            var invitation = invitations.First();
            Assert.IsTrue(invitations.Count > 0);
            Assert.AreEqual(KnownTestData.InvitationTitle, invitation.Title);
            Assert.AreEqual(KnownTestData.InvitationDescription, invitation.Description);
        }

        [TestMethod]
        public async Task CreateInvitation_AsPlanner_ShouldCreateInvitation()
        {
            // Act
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                "Title",
                "Description",
                "Location",
                DisciplineType.DP,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                Participants,
                McPkgScope,
                null
            );

            // Assert
            Assert.IsTrue(id > 0);
        }

        [TestMethod]
        public async Task GetAttachments_AsPlanner_ShouldGetAttachments()
        {
            // Act
            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                ViewerClient(TestFactory.PlantWithAccess),
                InitialInvitationId);

            // Assert
            Assert.IsNotNull(attachmentDtos);
            Assert.IsTrue(attachmentDtos.Count > 0);

            var invitationAttachment = attachmentDtos.Single(a => a.Id == AttachmentId);
            Assert.IsNotNull(invitationAttachment.FileName);
            Assert.IsNotNull(invitationAttachment.RowVersion);
        }

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldDeleteAttachment()
        {
            // Arrange
            var plannerClient = PlannerClient(TestFactory.PlantWithAccess);
            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                plannerClient,
                InitialInvitationId);
            var attachment = attachmentDtos.Single(t => t.Id == AttachmentId);

            // Act
            await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                plannerClient,
                InitialInvitationId,
                AttachmentId,
                attachment.RowVersion);

            // Assert
            attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                plannerClient,
                InitialInvitationId);
            Assert.IsNull(attachmentDtos.SingleOrDefault(m => m.Id == AttachmentId));
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
