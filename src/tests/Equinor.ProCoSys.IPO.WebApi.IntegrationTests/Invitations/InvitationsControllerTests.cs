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
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToSignId);

            var participantPerson = invitation.Participants
                .Single(p => p.Organization == Organization.TechnicalIntegrity).Person;

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.SignPunchOutAsync(
                    UserType.Signer,
                    TestFactory.PlantWithAccess,
                    invitationToSignId,
                    participantPerson.Person.Id,
                    participantPerson.Person.RowVersion);

            // Assert
            var signedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToSignId);

            var signerParticipant = signedInvitation.Participants.Single(p => p.Person?.Person.Id == participantPerson.Person.Id);
            Assert.IsNotNull(signerParticipant.SignedAtUtc);
            Assert.AreEqual("SigurdUserName", signerParticipant.SignedBy);
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task CompletePunchOut_AsCompleter_ShouldCompletePunchOut()
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

            var completerPerson = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor).Person;

                var completePunchOutDto = new CompletePunchOutDto
                {
                    InvitationRowVersion = invitation.RowVersion,
                    ParticipantRowVersion = completerPerson.Person.RowVersion,
                    Participants = new List<ParticipantToChangeDto>
                    {
                        new ParticipantToChangeDto
                        {
                            Id = completerPerson.Person.Id,
                            Note = "Some note about the punch round or attendee",
                            RowVersion = completerPerson.Person.RowVersion,
                            Attended = true
                        }
                    }
                };

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId,
                completePunchOutDto);

            // Assert
            var completedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId);

            var completingParticipant =
                completedInvitation.Participants.Single(p => p.Person?.Person.Id == completerPerson.Person.Id);
            Assert.AreEqual(IpoStatus.Completed, completedInvitation.Status);
            Assert.IsNotNull(completingParticipant.SignedAtUtc);
            Assert.AreEqual(_conradContractor.UserName, completingParticipant.SignedBy);
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsAccepter_ShouldAcceptPunchOut()
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
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var completerPerson = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor).Person;

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerPerson.Person.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                {
                    new ParticipantToChangeDto
                    {
                        Id = completerPerson.Person.Id,
                        Note = "Some note about the punch out round or attendee",
                        RowVersion = completerPerson.Person.RowVersion,
                        Attended = true
                    }
                }
            };

            // Punch round must be completed before it can be accepted
            var newRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                completePunchOutDto);

            var accepterPerson = invitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany).Person;

            var acceptPunchOutDto = new AcceptPunchOutDto
            {
                InvitationRowVersion = newRowVersion,
                ParticipantRowVersion = accepterPerson.Person.RowVersion,
                Participants = new List<ParticipantToUpdateNoteDto>
                {
                    new ParticipantToUpdateNoteDto
                    {
                        Id = accepterPerson.Person.Id,
                        Note = "Some note about the punch out round or attendee",
                        RowVersion = accepterPerson.Person.RowVersion
                    }
                }
            };

            // Act
            newRowVersion = await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Accepter,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                acceptPunchOutDto);

            // Assert
            var acceptedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var acceptingParticipant =
                acceptedInvitation.Participants.Single(p => p.Person?.Person.Id == accepterPerson.Person.Id);
            Assert.AreEqual(IpoStatus.Accepted, acceptedInvitation.Status);
            Assert.IsNotNull(acceptingParticipant.SignedAtUtc);
            Assert.AreEqual("ConnieUserName", acceptingParticipant.SignedBy);
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task UnAcceptPunchOut_AsAccepter_ShouldUnAcceptPunchOut()
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
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            var completerPerson = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor).Person;

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerPerson.Person.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                    {
                        new ParticipantToChangeDto
                        {
                            Id = completerPerson.Person.Id,
                            Note = "Some note about the punch out round or attendee",
                            RowVersion = completerPerson.Person.RowVersion,
                            Attended = true
                        }
                    }
            };

            // Punch round must be completed before it can be accepted
            var newInvitationRowVersion = await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                completePunchOutDto);

            var accepterPerson = invitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany).Person;

            var acceptPunchOutDto = new AcceptPunchOutDto
            {
                InvitationRowVersion = newInvitationRowVersion,
                ParticipantRowVersion = accepterPerson.Person.RowVersion,
                Participants = new List<ParticipantToUpdateNoteDto>
                    {
                        new ParticipantToUpdateNoteDto
                        {
                            Id = accepterPerson.Person.Id,
                            Note = "Some note about the punch out round or attendee",
                            RowVersion = accepterPerson.Person.RowVersion
                        }
                    }
            };

            // Punch round must be accepted before it can be unaccepted
            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Accepter,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                acceptPunchOutDto);

            invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            accepterPerson = invitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany).Person;

            var unAcceptPunchOutDto = new UnAcceptPunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = accepterPerson.Person.RowVersion,
                ObjectGuid = invitation.ObjectGuid
            };

            // Act
            var newRowVersion = await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Accepter,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId,
                unAcceptPunchOutDto);

            // Assert
            var unAcceptedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToUnAcceptId);

            var unAcceptingParticipant =
                unAcceptedInvitation.Participants.Single(p => p.Person?.Person.Id == accepterPerson.Person.Id);
            Assert.AreEqual(IpoStatus.Completed, unAcceptedInvitation.Status);
            Assert.IsNull(unAcceptingParticipant.SignedAtUtc);
            Assert.IsNull(unAcceptingParticipant.SignedBy);
            AssertRowVersionChange(invitation.RowVersion, newRowVersion);
        }

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsCompleter_ShouldChangeAttendedStatus()
        {
            //Arrange
            const string UpdatedNote = "Updated note about attendee";
            
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
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            var completerPerson = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor).Person;

                var completePunchOutDto = new CompletePunchOutDto
                {
                    InvitationRowVersion = invitation.RowVersion,
                    ParticipantRowVersion = completerPerson.Person.RowVersion,
                    Participants = new List<ParticipantToChangeDto>
                    {
                        new ParticipantToChangeDto
                        {
                            Id = completerPerson.Person.Id,
                            Note = "Some note about the punch round or attendee",
                            RowVersion = completerPerson.Person.RowVersion,
                            Attended = true
                        }
                    }
                };

                await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                    UserType.Completer,
                    TestFactory.PlantWithAccess,
                    invitationToChangeId,
                    completePunchOutDto);

                var participantToChangeDto = new[]
                {
                    new ParticipantToChangeDto
                    {
                        Id = completerPerson.Person.Id,
                        Attended = false,
                        Note = UpdatedNote,
                        RowVersion = completerPerson.Person.RowVersion
                    }
                };

            //Act
            await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                invitationToChangeId,
                participantToChangeDto);

            //Assert
            var invitationWithUpdatedAttendedStatus = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            var participant =
                invitationWithUpdatedAttendedStatus.Participants.Single(p => p.Person?.Person.Id == completerPerson.Person.Id);

            Assert.AreEqual(UpdatedNote, participant.Note);
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
