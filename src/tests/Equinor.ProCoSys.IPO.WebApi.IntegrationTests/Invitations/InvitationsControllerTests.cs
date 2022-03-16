using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
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

            var signerParticipant = signedInvitation.Participants.Single(p => p.Id == participant.Person.Id);
            Assert.IsNotNull(signerParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, signerParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(editInvitationDto.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnsignPunchOut_AsSigner_ShouldUnsignPunchOut()
        {
            // Arrange
            var (invitationToSignAndUnsignId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participantsForSigning);

            var participant = editInvitationDto.UpdatedParticipants.Single(p => p.Organization == Organization.TechnicalIntegrity);

            var currentRowVersion = await InvitationsControllerTestsHelper.SignPunchOutAsync(
                    UserType.Signer,
                    TestFactory.PlantWithAccess,
                    invitationToSignAndUnsignId,
                    participant.Person.Id,
                    participant.Person.RowVersion);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnsignPunchOutAsync(
                    UserType.Signer,
                    TestFactory.PlantWithAccess,
                    invitationToSignAndUnsignId,
                    participant.Person.Id,
                    currentRowVersion);

            // Assert
            var unsignedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToSignAndUnsignId);

            var signerParticipant = unsignedInvitation.Participants.Single(p => p.Id == participant.Person.Id);
            Assert.IsNull(signerParticipant.SignedAtUtc);
            Assert.IsNull(signerParticipant.SignedBy);
            AssertRowVersionChange(currentRowVersion, newRowVersion);
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
                completedInvitation.Participants.Single(p => p.Id == completePunchOutDto.Participants.Single().Id);
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
                unCompletedInvitation.Participants.Single(p => p.Id == unCompleterParticipant.Id);
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
                acceptedInvitation.Participants.Single(p => p.Id == acceptPunchOutDto.Participants.Single().Id);
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
                unAcceptedInvitation.Participants.Single(p => p.Id == unAccepterParticipant.Id);
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
                invitationWithUpdatedAttendedStatus.Participants.Single(p => p.Id == completerParticipant.Id);

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
            var originalParticipants = _participants;
            AssertParticipants(invitation, originalParticipants);
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
                    ExternalEmail = new CreateExternalEmailDto
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
        public async Task EditInvitation_AsPlanner_ShouldUpdateParticipant()
        {
            // Arrange
            var participants = new List<CreateParticipantsDto>(_participants);
            const string email1 = "knut1@test.com";
            const string email2 = "knut2@test.com";
            participants.Add(
                new CreateParticipantsDto
                {
                    Organization = Organization.External,
                    ExternalEmail = new CreateExternalEmailDto
                    {
                        Email = email1
                    },
                    SortKey = 3
                });
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(participants);
            Assert.AreEqual(3, editInvitationDto.UpdatedParticipants.Count());
            
            var editParticipants = editInvitationDto.UpdatedParticipants.ElementAt(2);
            Assert.AreEqual(email1, editParticipants.ExternalEmail.Email);
            editParticipants.ExternalEmail.Email = email2;

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

            Assert.AreEqual(_mcPkgScope.Count, updatedInvitation.McPkgScope.Count());
            Assert.AreEqual(3, updatedInvitation.Participants.Count());
            Assert.AreEqual(email2, updatedInvitation.Participants.ElementAt(2).ExternalEmail.ExternalEmail);
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldUpdateParticipantOrganization()
        {
            // Arrange
            var participants = new List<CreateParticipantsDto>(_participants);
            const string email1 = "knut1@test.com";
            const string email2 = "knut2@test.com";
            const Organization org1 = Organization.External;
            const Organization org2 = Organization.TechnicalIntegrity;
            participants.Add(
                new CreateParticipantsDto
                {
                    Organization = org1,
                    ExternalEmail = new CreateExternalEmailDto
                    {
                        Email = email1
                    },
                    SortKey = 3
                });
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(participants);
            Assert.AreEqual(3, editInvitationDto.UpdatedParticipants.Count());
            var editParticipants = editInvitationDto.UpdatedParticipants.ElementAt(2);
            Assert.AreEqual(email1, editParticipants.ExternalEmail.Email);
            Assert.AreEqual(org1, editParticipants.Organization);

            editParticipants.Organization = org2;
            editParticipants.ExternalEmail = null;
            var editInvitedPersonDto = new EditInvitedPersonDto
            {
                AzureOid = Guid.NewGuid(),
                Email = email2
            };
            editParticipants.Person = editInvitedPersonDto;

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                        TestFactory.PlantWithAccess,
                        editInvitedPersonDto.AzureOid.ToString(),
                        "IPO",
                        new List<string> { "SIGN" }))
                .Returns(Task.FromResult(new ProCoSysPerson
                {
                    AzureOid = editInvitedPersonDto.AzureOid.ToString(),
                    Email = editInvitedPersonDto.Email,
                    FirstName = "Ola",
                    LastName = "Nordmann",
                    UserName = "UserName"
                }));

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

            Assert.AreEqual(_mcPkgScope.Count, updatedInvitation.McPkgScope.Count());
            Assert.AreEqual(3, updatedInvitation.Participants.Count());
            Assert.AreEqual(org2, updatedInvitation.Participants.ElementAt(2).Organization);
            Assert.IsNull(updatedInvitation.Participants.ElementAt(2).ExternalEmail);
            Assert.IsNotNull(updatedInvitation.Participants.ElementAt(2).Person);
            Assert.AreEqual(email2, updatedInvitation.Participants.ElementAt(2).Person.Email);
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
                ExternalEmail = new EditExternalEmailDto
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

        [TestMethod]
        public async Task CancelPunchOut_AsContractor_ShouldCancelPunchOut()
        {
            // Arrange
            var (invitationToCancelId, cancelPunchOutDto) = await CreateValidCancelPunchOutDtoAsync(_participants);

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonInFunctionalRoleAsync(
                        TestFactory.PlantWithAccess,
                        _contractor.AsProCoSysPerson().AzureOid,
                        "FRCA"))
                .Returns(Task.FromResult(_contractor.AsProCoSysPerson()));

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Contractor,
                TestFactory.PlantWithAccess,
                invitationToCancelId,
                cancelPunchOutDto);

            // Assert
            var canceledInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Contractor,
                TestFactory.PlantWithAccess,
                invitationToCancelId);

            Assert.AreEqual(IpoStatus.Canceled, canceledInvitation.Status);
            AssertRowVersionChange(cancelPunchOutDto.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnsignPunchOut_AsAdmin_ShouldUnsignPunchOut()
        {
            // Arrange
            var (invitationToSignAndUnsignId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participantsForSigning);

            var participant = editInvitationDto.UpdatedParticipants.Single(p => p.Organization == Organization.TechnicalIntegrity);

            var currentRowVersion = await InvitationsControllerTestsHelper.SignPunchOutAsync(
                    UserType.Signer,
                    TestFactory.PlantWithAccess,
                    invitationToSignAndUnsignId,
                    participant.Person.Id,
                    participant.Person.RowVersion);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnsignPunchOutAsync(
                    UserType.Admin,
                    TestFactory.PlantWithAccess,
                    invitationToSignAndUnsignId,
                    participant.Person.Id,
                    currentRowVersion);

            // Assert
            var unsignedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToSignAndUnsignId);

            var signerParticipant = unsignedInvitation.Participants.Single(p => p.Id == participant.Person.Id);
            Assert.IsNull(signerParticipant.SignedAtUtc);
            Assert.IsNull(signerParticipant.SignedBy);
            AssertRowVersionChange(currentRowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnCompletePunchOut_AsAdmin_ShouldUnCompletePunchOut()
        {
            // Arrange
            var (invitationToUnCompleteId, unCompletePunchOutDto) = await CreateValidUnCompletePunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Admin,
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
                unCompletedInvitation.Participants.Single(p => p.Id == unCompleterParticipant.Id);
            Assert.AreEqual(IpoStatus.Planned, unCompletedInvitation.Status);
            Assert.IsNull(unCompletedInvitation.CompletedBy);
            Assert.IsNull(unCompletedInvitation.CompletedAtUtc);
            Assert.IsNull(unCompletingParticipant.SignedAtUtc);
            Assert.IsNull(unCompletingParticipant.SignedBy);
            AssertRowVersionChange(unCompletePunchOutDto.InvitationRowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnAcceptPunchOut_AsAdmin_ShouldUnAcceptPunchOut()
        {
            // Arrange
            var (invitationToUnAcceptId, unAcceptPunchOutDto) = await CreateValidUnAcceptPunchOutDtoAsync(_participantsForSigning);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Admin,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                unAcceptPunchOutDto);

            // Assert
            var unAcceptedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Admin,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            var unAccepterParticipant = unAcceptedInvitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var unAcceptingParticipant =
                unAcceptedInvitation.Participants.Single(p => p.Id == unAccepterParticipant.Id);
            Assert.AreEqual(IpoStatus.Completed, unAcceptedInvitation.Status);
            Assert.IsNull(unAcceptingParticipant.SignedAtUtc);
            Assert.IsNull(unAcceptingParticipant.SignedBy);
            AssertRowVersionChange(unAcceptPunchOutDto.InvitationRowVersion, newRowVersion);
        }

        private void AssertParticipants(InvitationDto invitation, List<CreateParticipantsDto> originalParticipants)
        {
            Assert.IsNotNull(invitation.Participants);
            Assert.AreEqual(_participants.Count(), invitation.Participants.Count());

            var originalFunctionalRoleParticipants = originalParticipants.Where(p => p.FunctionalRole != null).ToList();
            var functionalRoleParticipants = invitation.Participants.Where(p => p.FunctionalRole != null).ToList();
            AssertFunctionalRoleParticipants(originalFunctionalRoleParticipants, functionalRoleParticipants);

            var originalExternalEmailParticipants = originalParticipants.Where(p => p.ExternalEmail != null).ToList();
            var ExternalEmailParticipants = invitation.Participants.Where(p => p.ExternalEmail != null).ToList();

            AssertExternalEmailParticipants(originalExternalEmailParticipants, ExternalEmailParticipants);

            var originalPersonParticipants = originalParticipants.Where(p => p.Person != null).ToList();
            var PersonParticipants = invitation.Participants.Where(p => p.Person != null).ToList();

            AssertPersonParticipants(originalPersonParticipants, PersonParticipants);
        }

        private void AssertPersonParticipants(
            List<CreateParticipantsDto> originalPersonParticipants,
            List<GetInvitation.ParticipantDto> personParticipants)
        {
            Assert.AreEqual(originalPersonParticipants.Count(), personParticipants.Count());
            foreach (var originalPersonParticipant in originalPersonParticipants)
            {
                var personParticipant = personParticipants.SingleOrDefault(p => p.Person.AzureOid == originalPersonParticipant.Person.AzureOid);
                Assert.IsNotNull(personParticipant);
            }
        }

        private void AssertExternalEmailParticipants(
            List<CreateParticipantsDto> originalExternalEmailParticipants,
            List<GetInvitation.ParticipantDto> externalEmailParticipants)
        {
            Assert.AreEqual(originalExternalEmailParticipants.Count(), externalEmailParticipants.Count());
            foreach (var originalExternalEmailParticipant in originalExternalEmailParticipants)
            {
                var externalEmailParticipant = externalEmailParticipants.SingleOrDefault(p => p.ExternalEmail.ExternalEmail == originalExternalEmailParticipant.ExternalEmail.Email);
                Assert.IsNotNull(externalEmailParticipant);
            }
        }

        private static void AssertFunctionalRoleParticipants(
            List<CreateParticipantsDto> originalFunctionalRoleParticipants,
            List<GetInvitation.ParticipantDto> functionalRoleParticipants)
        {
            Assert.AreEqual(originalFunctionalRoleParticipants.Count(), functionalRoleParticipants.Count());
            foreach (var originalFunctionalRoleParticipant in originalFunctionalRoleParticipants)
            {
                var functionalRoleParticipant = functionalRoleParticipants.SingleOrDefault(p => p.FunctionalRole.Code == originalFunctionalRoleParticipant.FunctionalRole.Code);
                Assert.IsNotNull(functionalRoleParticipant);
                Assert.AreEqual(originalFunctionalRoleParticipant.FunctionalRole.Persons.Count(), functionalRoleParticipant.FunctionalRole.Persons.Count());
            }
        }
    }
}
