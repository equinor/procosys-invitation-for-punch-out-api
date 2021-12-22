using System;
using System.Collections.Generic;
using System.Linq;
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
                InitialMdpInvitationId, 
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

        [TestMethod]
        public async Task GetInvitation_AsSigner_ShouldReturnNotFound_WhenUnknownId()
            => await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                HttpStatusCode.NotFound);
        #endregion

        #region ExportInvitations
        [TestMethod]
        public async Task ExportInvitations_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.ExportInvitationsAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task ExportInvitations_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.ExportInvitationsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task ExportInvitations_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.ExportInvitationsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Forbidden);
        #endregion

        #region GetInvitations
        [TestMethod]
        public async Task GetInvitations_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetInvitationsAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetInvitations_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetInvitationsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                TestFactory.ProjectWithAccess,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetInvitations_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetInvitationsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Forbidden);
        #endregion

        #region GetInvitationsByCommPkgNo
        [TestMethod]
        public async Task GetInvitationsByCommPkgNo_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetInvitationsByCommPkgNoAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "CommPkgNo1",
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetInvitationsByCommPkgNo_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetInvitationsByCommPkgNoAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                "CommPkgNo1",
                TestFactory.ProjectWithAccess,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetInvitationsByCommPkgNo_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetInvitationsByCommPkgNoAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                "CommPkgNo1",
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Forbidden);
        #endregion

        #region GetLatestMdpIpoOnCommPkgs
        [TestMethod]
        public async Task GetLatestMdpIpoOnCommPkgsAsync_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetLatestMdpIpoOnCommPkgsAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                new List<string>{"CommPkgNo1"},
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetLatestMdpIpoOnCommPkgsAsync_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetLatestMdpIpoOnCommPkgsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                new List<string> { "CommPkgNo1" },
                TestFactory.ProjectWithAccess,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetLatestMdpIpoOnCommPkgsAsync_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetLatestMdpIpoOnCommPkgsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                new List<string> { "CommPkgNo1" },
                TestFactory.ProjectWithAccess,
                HttpStatusCode.Forbidden);
        #endregion

        #region Create
        [TestMethod]
        public async Task CreateInvitation_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "Title",
                "Description",
                "Location",
                DisciplineType.DP,
                _invitationStartTime, 
                _invitationEndTime, 
                _participants,
                _mcPkgScope,
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
                _invitationStartTime, 
                _invitationEndTime, 
                _participants,
                _mcPkgScope,
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
                _invitationStartTime,
                _invitationEndTime,
                _participants,
                _mcPkgScope,
                null,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CreateInvitation_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithoutAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participants,
                _mcPkgScope,
                null,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CreateInvitation_AsViewer_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithoutAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participants,
                _mcPkgScope,
                null,
                HttpStatusCode.Forbidden);
        #endregion

        #region Edit
        [TestMethod]
        public async Task EditInvitation_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess, 
                9999, 
                new EditInvitationDto(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task EditInvitation_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new EditInvitationDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.UnknownPlant,
                9999,
                new EditInvitationDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task EditInvitation_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new EditInvitationDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task EditInvitation_AsViewer_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                9999,
                new EditInvitationDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task EditInvitation_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new EditInvitationDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            var (_, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participants);
            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                38934,
                editInvitationDto,
                HttpStatusCode.BadRequest,
                "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldReturnBadRequest_WhenUnknownParticipantId()
        {
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participants);
            editInvitationDto.UpdatedParticipants.First(p => p.Person != null).Person.Id = 23451;
            
            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                editInvitationDto,
                HttpStatusCode.BadRequest,
                "Participant with ID does not exist on invitation!");
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
        {
            // Arrange
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participants);
            editInvitationDto.RowVersion = TestFactory.WrongButValidRowVersion;
            
            // Act
            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                editInvitationDto,
                HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task EditInvitation_AsPlanner_ShouldReturnConflict_WhenWrongParticipantRowVersion()
        {
            // Arrange
            var (invitationId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participants);
            editInvitationDto.UpdatedParticipants.First(p => p.FunctionalRole != null).FunctionalRole.RowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                editInvitationDto,
                HttpStatusCode.Conflict);
        }
        #endregion

        #region Sign 
        [TestMethod]
        public async Task SignPunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                88,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task SignPunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                88,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Signer,
                TestFactory.UnknownPlant,
                9999,
                88,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task SignPunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                88,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task SignPunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                88,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (invitationToSignId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participantsForSigning);
            var participant = editInvitationDto.UpdatedParticipants.Single(p => p.Organization == Organization.TechnicalIntegrity);

            // Act
            await InvitationsControllerTestsHelper.SignPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           38934,
                           participant.Person.Id,
                           TestFactory.AValidRowVersion,
                           HttpStatusCode.BadRequest,
                           "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownParticipantId()
        {
            // Arrange
            var (invitationToSignId, editInvitationDto) = await CreateValidEditInvitationDtoAsync(_participantsForSigning);
            var participant = editInvitationDto.UpdatedParticipants.Single(p => p.Organization == Organization.TechnicalIntegrity);

            await InvitationsControllerTestsHelper.SignPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           invitationToSignId,
                           38934,
                           TestFactory.AValidRowVersion,
                           HttpStatusCode.BadRequest,
                           "Participant with ID does not exist on invitation!");
        }

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
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
                    TestFactory.WrongButValidRowVersion,
                    HttpStatusCode.Conflict);
        }
        #endregion

        #region Complete
        [TestMethod]
        public async Task CompletePunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.UnknownPlant,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CompletePunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CompletePunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CompletePunchOut_AsViewer_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CompletePunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (_, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(_participantsForSigning);

            // Act
            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                completePunchOutDto,
                HttpStatusCode.BadRequest,
                "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownParticipantId()
        {
            // Arrange
            var (invitationToCompleteId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(_participantsForSigning);
            completePunchOutDto.Participants.Single().Id = 23423;
            
            // Act
            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId,
                completePunchOutDto,
                HttpStatusCode.BadRequest,
                "Participant with ID does not exist on invitation!");
        }

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
        {
            // Arrange
            var (invitationToCompleteId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(_participantsForSigning);
            completePunchOutDto.InvitationRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId,
                completePunchOutDto,
                HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldReturnConflict_WhenWrongParticipantRowVersion()
        {
            // Arrange
            var (invitationToCompleteId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(_participantsForSigning);
            completePunchOutDto.Participants.Single().RowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteId,
                completePunchOutDto,
                HttpStatusCode.Conflict);
        }

        #endregion

        #region UnComplete
        [TestMethod]
        public async Task UnCompletePunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                new UnCompletePunchOutDto(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task UnCompletePunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new UnCompletePunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UnCompletePunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Signer,
                TestFactory.UnknownPlant,
                9999,
                new UnCompletePunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UnCompletePunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new UnCompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UnCompletePunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                new UnCompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UnCompletePunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (_, unCompletePunchOutDto) = await CreateValidUnCompletePunchOutDtoAsync(_participantsForSigning);
            unCompletePunchOutDto.InvitationRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           9999,
                           unCompletePunchOutDto,
                           HttpStatusCode.BadRequest,
                           "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task UnCompletePunchOut_AsSigner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
        {
            // Arrange
            var (invitationToUnCompleteId, unCompletePunchOutDto) = await CreateValidUnCompletePunchOutDtoAsync(_participantsForSigning);
            unCompletePunchOutDto.InvitationRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompleteId,
                unCompletePunchOutDto,
                HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task UnCompletePunchOut_AsSigner_ShouldReturnConflict_WhenWrongParticipantRowVersion()
        {
            // Arrange
            var (invitationToUnCompleteId, unCompletePunchOutDto) = await CreateValidUnCompletePunchOutDtoAsync(_participantsForSigning);
            unCompletePunchOutDto.ParticipantRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToUnCompleteId,
                unCompletePunchOutDto,
                HttpStatusCode.Conflict);
        }
        #endregion

        #region Accept
        [TestMethod]
        public async Task AcceptPunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task AcceptPunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.UnknownPlant,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task AcceptPunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task AcceptPunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task AcceptPunchOut_AsViewer_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (_, acceptPunchOutDto) = await CreateValidAcceptPunchOutDtoAsync(_participantsForSigning);

            // Act
            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           9999,
                           acceptPunchOutDto,
                           HttpStatusCode.BadRequest,
                           "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownParticipantId()
        {
            // Arrange
            var (invitationToAcceptId, acceptPunchOutDto) = await CreateValidAcceptPunchOutDtoAsync(_participantsForSigning);
            acceptPunchOutDto.Participants.Single().Id = 12399;

            // Act
            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           invitationToAcceptId,
                           acceptPunchOutDto,
                           HttpStatusCode.BadRequest,
                           "Participant with ID does not exist on invitation!");
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
        {
            // Arrange
            var (invitationToAcceptId, acceptPunchOutDto) = await CreateValidAcceptPunchOutDtoAsync(_participantsForSigning);
            acceptPunchOutDto.InvitationRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                acceptPunchOutDto,
                HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsSigner_ShouldReturnConflict_WhenWrongParticipantRowVersion()
        {
            // Arrange
            var (invitationToAcceptId, acceptPunchOutDto) = await CreateValidAcceptPunchOutDtoAsync(_participantsForSigning);
            acceptPunchOutDto.Participants.Single().RowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                acceptPunchOutDto,
                HttpStatusCode.Conflict);
        }
        #endregion

        #region UnAccept
        [TestMethod]
        public async Task UnAcceptPunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                new UnAcceptPunchOutDto(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task UnAcceptPunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new UnAcceptPunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UnAcceptPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.UnknownPlant,
                9999,
                new UnAcceptPunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UnAcceptPunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new UnAcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UnAcceptPunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                new UnAcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UnAcceptPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (_, unAcceptPunchOutDto) = await CreateValidUnAcceptPunchOutDtoAsync(_participantsForSigning);
            
            // Act
            await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           9999,
                           unAcceptPunchOutDto,
                           HttpStatusCode.BadRequest,
                           "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task UnAcceptPunchOut_AsSigner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
        {
            // Arrange
            var (invitationToUnAcceptId, unAcceptPunchOutDto) = await CreateValidUnAcceptPunchOutDtoAsync(_participantsForSigning);
            unAcceptPunchOutDto.InvitationRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           invitationToUnAcceptId,
                           unAcceptPunchOutDto,
                           HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task UnAcceptPunchOut_AsSigner_ShouldReturnConflict_WhenWrongParticipantRowVersion()
        {
            // Arrange
            var (invitationToUnAcceptId, unAcceptPunchOutDto) = await CreateValidUnAcceptPunchOutDtoAsync(_participantsForSigning);
            unAcceptPunchOutDto.ParticipantRowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           invitationToUnAcceptId,
                           unAcceptPunchOutDto,
                           HttpStatusCode.Conflict);
        }
        #endregion

        #region Cancel
        [TestMethod]
        public async Task CancelPunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                new CancelPunchOutDto(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task CancelPunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new CancelPunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CancelPunchOut_AsPlanner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Planner,
                TestFactory.UnknownPlant,
                9999,
                new CancelPunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CancelPunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new CancelPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CancelPunchOut_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new CancelPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CancelPunchOut_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (_, cancelPunchOutDto) = await CreateValidCancelPunchOutDtoAsync(_participantsForSigning);

            // Act
            await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                           UserType.Planner,
                           TestFactory.PlantWithAccess,
                           9999,
                           cancelPunchOutDto,
                           HttpStatusCode.BadRequest,
                           "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task CancelPunchOut_AsPlanner_ShouldReturnConflict_WhenWrongInvitationRowVersion()
        {
            // Arrange
            var (invitationToCancelId, cancelPunchOutDto) = await CreateValidCancelPunchOutDtoAsync(_participantsForSigning);
            cancelPunchOutDto.RowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                           UserType.Planner,
                           TestFactory.PlantWithAccess,
                           invitationToCancelId,
                           cancelPunchOutDto,
                           HttpStatusCode.Conflict);
        }
        #endregion

        #region ChangeAttendedStatusOnParticipants
        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                new ParticipantToChangeDto[1],
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new ParticipantToChangeDto[1],
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsPlanner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Planner,
                TestFactory.UnknownPlant,
                9999,
                new ParticipantToChangeDto[1],
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                new ParticipantToChangeDto[1],
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            // Arrange
            var (_, participantToChangeDtos) = await CreateValidParticipantToChangeDtosAsync(_participantsForSigning);

            // Act
            await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           9999,
                           participantToChangeDtos,
                           HttpStatusCode.BadRequest,
                           "Invitation with this ID does not exist!");
        }

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsSigner_ShouldReturnBadRequest_WhenUnknownParticipantId()
        {
            // Arrange
            var (invitationToChangeId, participantToChangeDtos) = await CreateValidParticipantToChangeDtosAsync(_participantsForSigning);
            participantToChangeDtos[0].Id = 290690;

            // Act
            await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           invitationToChangeId,
                           participantToChangeDtos,
                           HttpStatusCode.BadRequest,
                           "Participant with ID does not exist on invitation!");
        }

        [TestMethod]
        public async Task ChangeAttendedStatusOnParticipants_AsSigner_ShouldReturnConflict_WhenWrongParticipantRowVersion()
        {
            // Arrange
            var (invitationToChangeId, participantToChangeDtos) = await CreateValidParticipantToChangeDtosAsync(_participantsForSigning);
            participantToChangeDtos[0].RowVersion = TestFactory.WrongButValidRowVersion;

            // Act
            await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                           UserType.Signer,
                           TestFactory.PlantWithAccess,
                           invitationToChangeId,
                           participantToChangeDtos,
                           HttpStatusCode.Conflict);
        }
        #endregion

        #region UploadAttachment
        [TestMethod]
        public async Task UploadAttachment_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Anonymous,
                TestFactory.PlantWithAccess,
                9999,
                TestFile.NewFileToBeUploaded(),
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task UploadAttachment_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                TestFile.NewFileToBeUploaded(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.UnknownPlant,
                9999,
                TestFile.NewFileToBeUploaded(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UploadAttachment_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                TestFile.NewFileToBeUploaded(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                TestFile.NewFileToBeUploaded(),
                HttpStatusCode.BadRequest,
                "Invitation with this ID does not exist!");
        #endregion

        #region DeleteAttachment
        [TestMethod]
        public async Task DeleteAttachment_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task DeleteAttachment_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task DeleteAttachment_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownAttachmentId()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                123456,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "Attachment doesn't exist!");

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                _attachmentOnInitialMdpInvitation.Id,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "Invitation with this ID does not exist!");

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldReturnConflict_WhenWrongAttachmentRowVersion()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                TestFactory.WrongButValidRowVersion,
                HttpStatusCode.Conflict);
        #endregion

        #region GetAttachments
        [TestMethod]
        public async Task GetAttachments_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Anonymous, 
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetAttachments_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetAttachments_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetAttachments_AsPlanner_ShouldReturnNotFound_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                123456,
                HttpStatusCode.NotFound);
        #endregion

        #region GetAttachmentById
        [TestMethod]
        public async Task GetAttachmentById_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetAttachmentById_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetAttachmentById_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                _attachmentOnInitialMdpInvitation.Id,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetAttachmentById_AsPlanner_ShouldReturnNotFound_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                12456,
                _attachmentOnInitialMdpInvitation.Id,
                HttpStatusCode.NotFound);

        [TestMethod]
        public async Task GetAttachmentById_AsPlanner_ShouldReturnNotFound_WhenUnknownAttachmentId()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                12456,
                _attachmentOnInitialMdpInvitation.Id,
                HttpStatusCode.NotFound);
        #endregion

        #region AddComment
        [TestMethod]
        public async Task AddComment_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.AddCommentAsync(
                UserType.Anonymous,
                TestFactory.PlantWithAccess,
                9999,
                "comment",
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task AddComment_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.AddCommentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                "comment",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task AddComment_AsPlanner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.AddCommentAsync(
                UserType.Planner,
                TestFactory.UnknownPlant,
                9999,
                "comment",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task AddComment_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.AddCommentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                "comment",
                HttpStatusCode.Forbidden);


        [TestMethod]
        public async Task AddComment_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.AddCommentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                123456,
                "comment",
                HttpStatusCode.BadRequest,
                "Invitation with this ID does not exist");
        #endregion

        #region GetHistory
        [TestMethod]
        public async Task GetHistory_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetHistory_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialMdpInvitationId,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetHistory_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialMdpInvitationId,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetHistory_AsPlanner_ShouldReturnNotFound_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                123456,
                HttpStatusCode.NotFound);
        #endregion
    }
}
