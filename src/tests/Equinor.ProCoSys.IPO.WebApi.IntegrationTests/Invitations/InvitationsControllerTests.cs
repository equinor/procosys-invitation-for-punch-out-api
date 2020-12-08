using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
                UserType.Viewer,
                TestFactory.PlantWithAccess,
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
                UserType.Viewer,
                TestFactory.PlantWithAccess,
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
            const string Title = "InvitationTitle";
            const string Description = "InvitationDescription";
            const string Location = "InvitationLocation";

            // Act
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                Title,
                Description,
                Location,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participants,
                _mcPkgScope,
                null
            );

            // Assert
            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id);

            Assert.IsTrue(id > 0);
            Assert.IsNotNull(invitation);
            Assert.AreEqual(Title, invitation.Title);
            Assert.AreEqual(Description, invitation.Description);
            Assert.AreEqual(Location, invitation.Location);
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldEditInvitation()
        {
            // Arrange
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationToBeUpdatedTitle",
                "InvitationToBeUpdatedDescription",
                "InvitationToBeUpdatedLocation",
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participants,
                _mcPkgScope,
                null);

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id);

            invitation.Status = IpoStatus.Planned;

            var currentRowVersion = invitation.RowVersion;
            const string UpdatedTitle = "UpdatedInvitationTitle";
            const string UpdatedDescription = "UpdatedInvitationDescription";

            var editInvitationDto = new EditInvitationDto
            {
                Title = UpdatedTitle,
                Description = UpdatedDescription,
                StartTime = invitation.StartTimeUtc,
                EndTime = invitation.EndTimeUtc,
                Location = invitation.Location,
                ProjectName = invitation.ProjectName,
                RowVersion = invitation.RowVersion,
                UpdatedParticipants = ConvertToParticipantDtoEdit(invitation.Participants),
                UpdatedCommPkgScope = null,
                UpdatedMcPkgScope = _mcPkgScope
            };

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                id,
                editInvitationDto);

            // Assert
            var updatedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id);

            AssertRowVersionChange(currentRowVersion, newRowVersion);
            Assert.AreEqual(UpdatedTitle, updatedInvitation.Title);
            Assert.AreEqual(UpdatedDescription, updatedInvitation.Description);
        }

        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldUploadAttachment()
        {
            // Arrange
            var invitationAttachments = InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);
            var attachmentCount = invitationAttachments.Result.Count;

            // Act
            await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                FileToBeUploaded);

            // Assert
            invitationAttachments = InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);

            Assert.AreEqual(attachmentCount + 1, invitationAttachments.Result.Count);
        }

        [TestMethod]
        public async Task GetAttachment_AsViewer_ShouldGetAttachment()
        {
            // Arrange
            var invitationAttachments = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);

            Assert.AreNotEqual(invitationAttachments.Count, 0);

            // Act
            var attachmentDto = await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                invitationAttachments.First().Id);

            // Assert
            Assert.AreEqual(invitationAttachments.First().Id, attachmentDto.Id);
        }

        [TestMethod]
        public async Task GetAttachments_AsViewer_ShouldGetAttachments()
        {
            // Act
            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);

            // Assert
            Assert.IsNotNull(attachmentDtos);
            Assert.IsTrue(attachmentDtos.Count > 0);

            var invitationAttachment = attachmentDtos.Single(a => a.Id == _attachmentId);
            Assert.IsNotNull(invitationAttachment.FileName);
            Assert.IsNotNull(invitationAttachment.RowVersion);
        }

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldDeleteAttachment()
        {
            // Arrange
            await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                FileToBeUploaded2);

            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);
            var attachment = attachmentDtos.Single(t => t.FileName == FileToBeUploaded2.FileName);

            // Act
            await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                attachment.Id,
                attachment.RowVersion);

            // Assert
            attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);
            Assert.IsNull(attachmentDtos.SingleOrDefault(m => m.Id == attachment.Id));
        }

        private IEnumerable<ParticipantDtoEdit> ConvertToParticipantDtoEdit(IEnumerable<ParticipantDtoGet> participants)
        {
            var editVersionParticipantDtos = new List<ParticipantDtoEdit>();
            participants.ToList().ForEach(p => editVersionParticipantDtos.Add(
                new ParticipantDtoEdit
                {
                    ExternalEmail = p.ExternalEmail,
                    FunctionalRole = p.FunctionalRole,
                    Organization = p.Organization,
                    Person = p.Person?.Person,
                    SortKey = p.SortKey
                }));

            return editVersionParticipantDtos;
        }
    }
}
