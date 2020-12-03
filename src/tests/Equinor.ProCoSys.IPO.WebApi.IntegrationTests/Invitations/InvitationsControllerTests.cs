using System;
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
        public async Task EditInvitation_AsPlanner_ShouldEditInvitation()
        {
            // Arrange
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                "InvitationToBeUpdatedTitle",
                "InvitationToBeUpdatedDescription",
                "InvitationToBeUpdatedLocation",
                DisciplineType.DP,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                Participants,
                McPkgScope,
                null);

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                ViewerClient(TestFactory.PlantWithAccess),
                id);
            invitation.Status = IpoStatus.Planned;
            
            var CurrentRowVersion = invitation.RowVersion;
            const string UpdatedTitle = "UpdatedInvitationTitle";
            const string UpdatedDescription = "UpdatedInvitationDescription";
            var participant1 = Participants.First();
            var participant2 = Participants.Last();
            var updatedParticipants = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    Organization = participant1.Organization,
                    ExternalEmail = null,
                    Person = null,
                    SortKey = participant1.SortKey,
                    FunctionalRole = new FunctionalRoleDto
                    {
                        Code = participant1.FunctionalRole.Code,
                        Id = participant1.FunctionalRole.Id,
                        Persons = new List<PersonDto>(),
                        RowVersion = participant1.FunctionalRole.RowVersion
                    }
                },
                new ParticipantDto
                {
                    Organization = participant2.Organization,
                    ExternalEmail = null,
                    Person = new PersonDto
                    {
                        AzureOid = participant2.Person.AzureOid,
                        Email = participant2.Person.Email,
                        FirstName = participant2.Person.FirstName,
                        LastName = participant2.Person.LastName,
                        Id = participant2.Person.Id,
                        Required = participant2.Person.Required,
                        RowVersion = participant2.Person.RowVersion
                    },
                    SortKey = participant2.SortKey,
                    FunctionalRole = null
                }
            };

            var editInvitationDto = new EditInvitationDto
            {
                Title = UpdatedTitle,
                Description = UpdatedDescription,
                StartTime = invitation.StartTimeUtc,
                EndTime = invitation.EndTimeUtc,
                Location = invitation.Location,
                ProjectName = invitation.ProjectName,
                RowVersion = invitation.RowVersion,
                UpdatedParticipants = updatedParticipants,
                UpdatedCommPkgScope = null,
                UpdatedMcPkgScope = McPkgScope
            };

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.EditInvitationAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                id,
                editInvitationDto);

            var updatedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                ViewerClient(TestFactory.PlantWithAccess),
                id);

            // Assert
            AssertRowVersionChange(CurrentRowVersion, newRowVersion);
            Assert.AreEqual(UpdatedTitle, updatedInvitation.Title);
            Assert.AreEqual(UpdatedDescription, updatedInvitation.Description);
        }
         
        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldUploadAttachment()
        {
            // Arrange
            var invitationIdForAttachment = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                "InvitationForAttachmentTitle",
                "InvitationForAttachmentDescription",
                "InvitationForAttachmentLocation",
                DisciplineType.DP,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                Participants,
                McPkgScope,
                null);

            var invitationAttachments = InvitationsControllerTestsHelper.GetAttachmentsAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                invitationIdForAttachment);
            var attachmentCount = invitationAttachments.Result.Count;

            // Act
            await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                invitationIdForAttachment,
                FileToBeUploaded);

            // Assert
            invitationAttachments = InvitationsControllerTestsHelper.GetAttachmentsAsync(
                PlannerClient(TestFactory.PlantWithAccess),
                invitationIdForAttachment);

            Assert.AreEqual(attachmentCount + 1, invitationAttachments.Result.Count);
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

        [TestMethod]
        public async Task GetAttachment_AsViewer_ShouldGetAttachment()
        {
           // Act
           var invitationAttachments = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
               PlannerClient(TestFactory.PlantWithAccess),
               InitialInvitationId);

           Assert.AreNotEqual(invitationAttachments.Count, 0);

            var attachmentDto = await InvitationsControllerTestsHelper.GetAttachmentAsync(
                ViewerClient(TestFactory.PlantWithAccess),
                InitialInvitationId,
                invitationAttachments.First().Id);

            // Assert
            Assert.AreEqual(invitationAttachments.First().Id, attachmentDto.Id);
        }
    }
}
