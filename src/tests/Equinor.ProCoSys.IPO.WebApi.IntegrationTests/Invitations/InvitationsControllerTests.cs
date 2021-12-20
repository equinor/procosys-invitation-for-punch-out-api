﻿using System.Collections.Generic;
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
            var invitationToSignId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationForSigningTitle",
                "InvitationForSigningDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToSignId);

            var participant = invitation.Participants
                .Single(p => p.Organization == Organization.TechnicalIntegrity);

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.SignPunchOutAsync(
                    UserType.Signer,
                    TestFactory.PlantWithAccess,
                    invitationToSignId,
                    participant.Person.Person.Id,
                    participant.RowVersion);

            // Assert
            var signedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToSignId);

            var signerParticipant = signedInvitation.Participants.Single(p => p.Person?.Person.Id == participant.Person.Person.Id);
            Assert.IsNotNull(signerParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, signerParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldCompletePunchOut()
        {
            // Arrange
            var invitationToCompleteId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationForCompletingTitle",
                "InvitationForCompletingDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId);

            var completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                {
                    new ParticipantToChangeDto
                    {
                        Id = completerParticipant.Person.Person.Id,
                        Note = "Some note about the punch round or attendee",
                        RowVersion = completerParticipant.RowVersion,
                        Attended = true
                    }
                }
            };

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
                completedInvitation.Participants.Single(p => p.Person?.Person.Id == completerParticipant.Person.Person.Id);
            Assert.AreEqual(IpoStatus.Completed, completedInvitation.Status);
            Assert.IsNotNull(completingParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, completingParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnCompletePunchOut_AsSigner_ShouldUnCompletePunchOut()
        {
            // Arrange
            var invitationToUnCompletedId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationForUnCompletingTitle",
                "InvitationForUnCompletingDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompletedId);

            var completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                    {
                        new ParticipantToChangeDto
                        {
                            Id = completerParticipant.Person.Person.Id,
                            Note = "Some note about the punch out round or attendee",
                            RowVersion = completerParticipant.RowVersion,
                            Attended = true
                        }
                    }
            };

            // Punch round must be completed before it can be uncompleted
            var newInvitationRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompletedId,
                completePunchOutDto);


            invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompletedId);

            completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);
            var unCompletePunchOutDto = new UnCompletePunchOutDto
            {
                InvitationRowVersion = newInvitationRowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
            };

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompletedId,
                unCompletePunchOutDto);

            // Assert
            var unCompletedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompletedId);

            var unCompletingParticipant =
                unCompletedInvitation.Participants.Single(p => p.Person?.Person.Id == completerParticipant.Person.Person.Id);
            Assert.AreEqual(IpoStatus.Planned, unCompletedInvitation.Status);
            Assert.IsNull(unCompletedInvitation.CompletedBy);
            Assert.IsNull(unCompletedInvitation.CompletedAtUtc);
            Assert.IsNull(unCompletingParticipant.SignedAtUtc);
            Assert.IsNull(unCompletingParticipant.SignedBy);
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldAcceptPunchOut()
        {
            // Arrange
            var invitationToAcceptId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationForAcceptingTitle",
                "InvitationForAcceptingDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                {
                    new ParticipantToChangeDto
                    {
                        Id = completerParticipant.Person.Person.Id,
                        Note = "Some note about the punch out round or attendee",
                        RowVersion = completerParticipant.RowVersion,
                        Attended = true
                    }
                }
            };

            // Punch round must be completed before it can be accepted
            var newRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                completePunchOutDto);

            var accepterParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var acceptPunchOutDto = new AcceptPunchOutDto
            {
                InvitationRowVersion = newRowVersion,
                ParticipantRowVersion = accepterParticipant.RowVersion,
                Participants = new List<ParticipantToUpdateNoteDto>
                {
                    new ParticipantToUpdateNoteDto
                    {
                        Id = accepterParticipant.Person.Person.Id,
                        Note = "Some note about the punch out round or attendee",
                        RowVersion = accepterParticipant.RowVersion
                    }
                }
            };

            // Act
            newRowVersion = await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
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
                acceptedInvitation.Participants.Single(p => p.Person?.Person.Id == accepterParticipant.Person.Person.Id);
            Assert.AreEqual(IpoStatus.Accepted, acceptedInvitation.Status);
            Assert.IsNotNull(acceptingParticipant.SignedAtUtc);
            Assert.AreEqual(_sigurdSigner.Oid, acceptingParticipant.SignedBy.AzureOid.ToString());
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnAcceptPunchOut_AsSigner_ShouldUnAcceptPunchOut()
        {
            // Arrange
            var invitationToUnAcceptId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationForUnAcceptingTitle",
                "InvitationForUnAcceptingDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            var completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                    {
                        new ParticipantToChangeDto
                        {
                            Id = completerParticipant.Person.Person.Id,
                            Note = "Some note about the punch out round or attendee",
                            RowVersion = completerParticipant.RowVersion,
                            Attended = true
                        }
                    }
            };

            // Punch round must be completed before it can be accepted
            var newInvitationRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                completePunchOutDto);

            var accepterParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var acceptPunchOutDto = new AcceptPunchOutDto
            {
                InvitationRowVersion = newInvitationRowVersion,
                ParticipantRowVersion = accepterParticipant.RowVersion,
                Participants = new List<ParticipantToUpdateNoteDto>
                    {
                        new ParticipantToUpdateNoteDto
                        {
                            Id = accepterParticipant.Person.Person.Id,
                            Note = "Some note about the punch out round or attendee",
                            RowVersion = accepterParticipant.RowVersion
                        }
                    }
            };

            // Punch round must be accepted before it can be unaccepted
            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                acceptPunchOutDto);

            invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            accepterParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var unAcceptPunchOutDto = new UnAcceptPunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = accepterParticipant.RowVersion,
            };

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

            var unAcceptingParticipant =
                unAcceptedInvitation.Participants.Single(p => p.Person?.Person.Id == accepterParticipant.Person.Person.Id);
            Assert.AreEqual(IpoStatus.Completed, unAcceptedInvitation.Status);
            Assert.IsNull(unAcceptingParticipant.SignedAtUtc);
            Assert.IsNull(unAcceptingParticipant.SignedBy);
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsSigner_ShouldChangeAttendedStatus()
        {
            //Arrange
            const string updatedNote = "Updated note about attendee";
            
            var invitationToChangeId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationToChangeTitle",
                "InvitationToChangeDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            var completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                {
                    new ParticipantToChangeDto
                    {
                        Id = completerParticipant.Person.Person.Id,
                        Note = "Some note about the punch round or attendee",
                        RowVersion = completerParticipant.RowVersion,
                        Attended = true
                    }
                }
            };

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId,
                completePunchOutDto);

            invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);
            
            var participantToChangeDto = new[]
            {
                new ParticipantToChangeDto
                {
                    Id = completerParticipant.Person.Person.Id,
                    Attended = false,
                    Note = updatedNote,
                    RowVersion = completerParticipant.RowVersion
                }
            };

            //Act
            await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId,
                participantToChangeDto);

            //Assert
            var invitationWithUpdatedAttendedStatus = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            var participant =
                invitationWithUpdatedAttendedStatus.Participants.Single(p => p.Person?.Person.Id == completerParticipant.Person.Person.Id);

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
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationToBeUpdatedTitle",
                "InvitationToBeUpdatedDescription",
                InvitationLocation,
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
                FileToBeUploaded);

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
                InitialMdpInvitationId,
                FileToBeUploaded2);

            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId);
            var attachment = attachmentDtos.Single(t => t.FileName == FileToBeUploaded2.FileName);

            // Act
            await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                attachment.Id,
                attachment.RowVersion);

            // Assert
            attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
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
            var invitationToCancelId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationForCancelTitle",
                "InvitationForCancelDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null
            );

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationToCancelId);
            var cancelPunchOutDto = new CancelPunchOutDto
            {
                RowVersion = invitation.RowVersion
            };
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
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
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
                    SortKey = p.SortKey,
                    RowVersion = p.RowVersion
                }));

            return editVersionParticipantDtos;
        }
    }
}
