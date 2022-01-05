using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    [TestClass]
    public class InvitationsControllerTests : InvitationsControllerTestsBase
    {
        [TestMethod]
        public async Task GetInvitations_AsViewer_ShouldGetInvitations()
        {
            // Act
            var invitationResuls = await InvitationsControllerTestsHelper.GetInvitationsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);

            // Assert
            Assert.IsTrue(invitationResuls.Invitations.Count > 0);
            Assert.IsTrue(invitationResuls.MaxAvailable > 0);
        }

        [TestMethod]
        public async Task ExportInvitations_AsViewer_ShouldExportInvitations()
        {
            // Act
            var file = await InvitationsControllerTestsHelper.ExportInvitationsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);

            // Assert
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
        }

        [TestMethod]
        public async Task GetMdpInvitation_AsViewer_ShouldGetInvitation()
        {
            // Act
            var mdpInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);

            // Assert
            Assert.IsNotNull(mdpInvitation);
            Assert.IsTrue(mdpInvitation.CommPkgScope.Any());
            Assert.AreEqual(0, mdpInvitation.McPkgScope.Count());
            Assert.IsNotNull(mdpInvitation.RowVersion);
        }

        [TestMethod]
        public async Task GetDpInvitation_AsViewer_ShouldGetInvitation()
        {
            // Act
            var dpInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialDpInvitationId);

            // Assert
            Assert.IsNotNull(dpInvitation);
            Assert.IsTrue(dpInvitation.McPkgScope.Any());
            Assert.AreEqual(0, dpInvitation.CommPkgScope.Count());
            Assert.IsNotNull(dpInvitation.RowVersion);
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
            Assert.AreEqual($"{KnownTestData.InvitationTitle} MDP", invitation.Title);
            Assert.AreEqual(KnownTestData.InvitationDescription, invitation.Description);
        }

        [TestMethod]
        public async Task GetLatestMdpIpoOnCommPkgsAsync_AsViewer_ShouldGetCommPkgs()
        {
            // Act
            var commPkgs = await InvitationsControllerTestsHelper.GetLatestMdpIpoOnCommPkgsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                new List<string>{KnownTestData.CommPkgNo},
                TestFactory.ProjectWithAccess);

            // Assert
            Assert.IsTrue(commPkgs.Count > 0);
            var commPkg = commPkgs.First();
            Assert.AreEqual(KnownTestData.CommPkgNo, commPkg.CommPkgNo);
            Assert.IsFalse(commPkg.IsAccepted);
        }

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldSignPunchOut()
        {
            // Arrange
            var (invitationToSignId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participantsForSigning);

            var participant = editInvitationDto.UpdatedParticipants.Single(p => p.Organization == Organization.TechnicalIntegrity);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.SignPunchOutAsync(
                    UserType.Signer,
                    TestFactory.PlantWithAccess,
                    invitationToSignId,
                    participant.Person.Id,
                    participant.Person.RowVersion);

            // Assert
            var signedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToSignId);

            var signerParticipant = signedInvitation.Participants.Single(p => p.Person?.Id == participant.Person.Id);
            Assert.IsNotNull(signerParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, signerParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(editInvitationDto.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldCompletePunchOut()
        {
            // Arrange
            var (invitationToCompleteId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId,
                completePunchOutDto);

            // Assert
            var completedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId);

            var completingParticipant =
                completedInvitation.Participants.Single(p => p.Person?.Id == completePunchOutDto.Participants.Single().Id);
            Assert.AreEqual(IpoStatus.Completed, completedInvitation.Status);
            Assert.IsNotNull(completingParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, completingParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(completePunchOutDto.InvitationRowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnCompletePunchOut_AsSigner_ShouldUnCompletePunchOut()
        {
            // Arrange
            var (invitationToUnCompleteId, unCompletePunchOutDto) = await CreateValidUnCompletePunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompleteId,
                unCompletePunchOutDto);

            // Assert
            var unCompletedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompleteId);

            var unCompleterParticipant = unCompletedInvitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var unCompletingParticipant =
                unCompletedInvitation.Participants.Single(p => p.Person?.Id == unCompleterParticipant.Person.Id);
            Assert.AreEqual(IpoStatus.Planned, unCompletedInvitation.Status);
            Assert.IsNull(unCompletedInvitation.CompletedBy);
            Assert.IsNull(unCompletedInvitation.CompletedAtUtc);
            Assert.IsNull(unCompletingParticipant.SignedAtUtc);
            Assert.IsNull(unCompletingParticipant.SignedBy);
            AssertRowVersionChange(unCompletePunchOutDto.InvitationRowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldAcceptPunchOut()
        {
            // Arrange
            var (invitationToAcceptId, acceptPunchOutDto) = await CreateValidAcceptPunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                acceptPunchOutDto);

            // Assert
            var acceptedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var acceptingParticipant =
                acceptedInvitation.Participants.Single(p => p.Person?.Id == acceptPunchOutDto.Participants.Single().Id);
            Assert.AreEqual(IpoStatus.Accepted, acceptedInvitation.Status);
            Assert.IsNotNull(acceptingParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, acceptingParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(acceptPunchOutDto.InvitationRowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnAcceptPunchOut_AsSigner_ShouldUnAcceptPunchOut()
        {
            // Arrange
            var (invitationToUnAcceptId, unAcceptPunchOutDto) = await CreateValidUnAcceptPunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                unAcceptPunchOutDto);

            // Assert
            var unAcceptedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            var unAccepterParticipant = unAcceptedInvitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var unAcceptingParticipant =
                unAcceptedInvitation.Participants.Single(p => p.Person?.Id == unAccepterParticipant.Person.Id);
            Assert.AreEqual(IpoStatus.Completed, unAcceptedInvitation.Status);
            Assert.IsNull(unAcceptingParticipant.SignedAtUtc);
            Assert.IsNull(unAcceptingParticipant.SignedBy);
            AssertRowVersionChange(unAcceptPunchOutDto.InvitationRowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsSigner_ShouldChangeAttendedStatus()
        {
            //Arrange
            var (invitationToChangeId, participantToChangeDtos) = await CreateValidParticipantToChangeDtosAsync(_participantsForSigning);
            var updatedNote = participantToChangeDtos[0].Note;
            
            //Act
            await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId,
                participantToChangeDtos);

            //Assert
            var invitationWithUpdatedAttendedStatus = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            var completerParticipant = invitationWithUpdatedAttendedStatus.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var participant =
                invitationWithUpdatedAttendedStatus.Participants.Single(p => p.Person?.Id == completerParticipant.Person.Id);

            Assert.AreEqual(updatedNote, participant.Note);
            Assert.AreEqual(false, participant.Attended);
        }

        [TestMethod]
        public async Task CreateInvitation_AsPlanner_ShouldCreateInvitation()
        {
            const string Title = "InvitationTitle";
            const string Description = "InvitationDescription";

            // Act
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                Title,
                Description,
                InvitationLocation,
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
            Assert.AreEqual(InvitationLocation, invitation.Location);
            Assert.AreEqual(_mcPkgScope.Count, invitation.McPkgScope.Count());
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldEditInvitation()
        {
            // Arrange
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participants);

            var currentRowVersion = editInvitationDto.RowVersion;
            const string UpdatedTitle = "UpdatedInvitationTitle";
            const string UpdatedDescription = "UpdatedInvitationDescription";

            editInvitationDto.Title = UpdatedTitle;
            editInvitationDto.Description = UpdatedDescription;

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                editInvitationDto);

            // Assert
            var updatedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationId);

            AssertRowVersionChange(currentRowVersion, newRowVersion);
            Assert.AreEqual(UpdatedTitle, updatedInvitation.Title);
            Assert.AreEqual(UpdatedDescription, updatedInvitation.Description);
            Assert.AreEqual(_mcPkgScope.Count, updatedInvitation.McPkgScope.Count());
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldRemoveParticipant()
        {
            // Arrange
            var participants = new List<CreateParticipantsDto>(_participants);
            participants.Add(
                new CreateParticipantsDto
                {
                    Organization = Organization.External,
                    ExternalEmail = new CreateExternalEmailForDto
                    {
                        Email = "knut@test.com"
                    },
                    SortKey = 3
                });
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(participants);
            Assert.AreEqual(3, editInvitationDto.UpdatedParticipants.Count());

            editInvitationDto.UpdatedParticipants = editInvitationDto.UpdatedParticipants.Take(2);

            const string UpdatedTitle = "UpdatedInvitationTitle";
            const string UpdatedDescription = "UpdatedInvitationDescription";

            editInvitationDto.Title = UpdatedTitle;
            editInvitationDto.Description = UpdatedDescription;

            // Act
            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                editInvitationDto);

            // Assert
            var updatedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationId);

            Assert.AreEqual(2, updatedInvitation.Participants.Count());
            Assert.AreEqual(UpdatedTitle, updatedInvitation.Title);
            Assert.AreEqual(UpdatedDescription, updatedInvitation.Description);
            Assert.AreEqual(_mcPkgScope.Count, updatedInvitation.McPkgScope.Count());
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldAddParticipant()
        {
            // Arrange
            var participants = new List<CreateParticipantsDto>(_participants);
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(participants);
            Assert.AreEqual(2, editInvitationDto.UpdatedParticipants.Count());

            var updatedParticipants = new List<EditParticipantsDto>(editInvitationDto.UpdatedParticipants);
            updatedParticipants.Add(new EditParticipantsDto
            {
                Organization = Organization.External,
                ExternalEmail = new EditExternalEmailForDto
                {
                    Email = "knut@test.com"
                },
                SortKey = 3
            });
            editInvitationDto.UpdatedParticipants = updatedParticipants;

            const string UpdatedTitle = "UpdatedInvitationTitle";
            const string UpdatedDescription = "UpdatedInvitationDescription";

            editInvitationDto.Title = UpdatedTitle;
            editInvitationDto.Description = UpdatedDescription;

            // Act
            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                editInvitationDto);

            // Assert
            var updatedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationId);

            Assert.AreEqual(3, updatedInvitation.Participants.Count());
            Assert.AreEqual(UpdatedTitle, updatedInvitation.Title);
            Assert.AreEqual(UpdatedDescription, updatedInvitation.Description);
            Assert.AreEqual(_mcPkgScope.Count, updatedInvitation.McPkgScope.Count());
        }

        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldUploadAttachment()
        {
            // Arrange
            var invitationAttachments = InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);
            var attachmentCount = invitationAttachments.Result.Count;

            // Act
            await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                TestFile.NewFileToBeUploaded());

            // Assert
            invitationAttachments = InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);

            Assert.AreEqual(attachmentCount + 1, invitationAttachments.Result.Count);
        }

        [TestMethod]
        public async Task GetAttachment_AsViewer_ShouldGetAttachment()
        {
            // Arrange
            var invitationAttachments = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);

            Assert.AreNotEqual(invitationAttachments.Count, 0);

            // Act
            var attachmentDto = await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
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
                InitialMdpInvitationId);

            // Assert
            Assert.IsNotNull(attachmentDtos);
            Assert.IsTrue(attachmentDtos.Count > 0);

            var invitationAttachment = attachmentDtos.Single(a => a.Id == _attachmentOnInitialMdpInvitation.Id);
            Assert.IsNotNull(invitationAttachment.FileName);
            Assert.IsNotNull(invitationAttachment.RowVersion);
        }

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldDeleteAttachment()
        {
            // Arrange
            var attachment = await UploadAttachmentAsync(InitialMdpInvitationId);

            // Act
            await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                attachment.Id,
                attachment.RowVersion);

            // Assert
            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);
            Assert.IsNull(attachmentDtos.SingleOrDefault(m => m.Id == attachment.Id));
        }

        [TestMethod]
        public async Task GetComments_AsViewer_ShouldGetComments()
        {
            // Act
            var commentDtos = await InvitationsControllerTestsHelper.GetCommentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);

            // Assert
            Assert.IsNotNull(commentDtos);
            Assert.IsTrue(commentDtos.Count > 0);

            var comment = commentDtos.Single(c => c.Id == _commentId);
            Assert.IsNotNull(comment.Comment);
            Assert.IsNotNull(comment.RowVersion);
        }

        [TestMethod]
        public async Task AddComment_AsPlanner_ShouldAddComment()
        {
            // Arrange
            var invitationComments = InvitationsControllerTestsHelper.GetCommentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);
            var commentsCount = invitationComments.Result.Count;

            // Act
            await InvitationsControllerTestsHelper.AddCommentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                "comment on the IPO");

            // Assert
            invitationComments = InvitationsControllerTestsHelper.GetCommentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);

            Assert.AreEqual(commentsCount + 1, invitationComments.Result.Count);
        }

        [TestMethod]
        public async Task GetHistory_AsViewer_ShouldGetHistory()
        {
            // Act
            var historyDtos = await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);

            // Assert
            Assert.IsNotNull(historyDtos);
            Assert.IsTrue(historyDtos.Count > 0);

            var historyEvent = historyDtos.First();
            Assert.IsNotNull(historyEvent.Description);
            Assert.IsNotNull(historyEvent.EventType);
        }
        
        [TestMethod]
        public async Task CancelPunchOut_AsPlanner_ShouldCancelPunchOut()
        {
            // Arrange
            var (invitationToCancelId, cancelPunchOutDto) = await CreateValidCancelPunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationToCancelId,
                cancelPunchOutDto);

            // Assert
            var canceledInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationToCancelId);

            Assert.AreEqual(IpoStatus.Canceled, canceledInvitation.Status);
            AssertRowVersionChange(cancelPunchOutDto.RowVersion, newRowVersion);
        }
    }
}
