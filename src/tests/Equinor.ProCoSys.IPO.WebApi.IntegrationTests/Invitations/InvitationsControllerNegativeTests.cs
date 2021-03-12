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
                InitialInvitationId, 
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

        [TestMethod]
        public async Task CreateInvitation_AsCompleter_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Completer,
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
        public async Task CreateInvitation_AsAccepter_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Accepter,
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
        public async Task EditInvitation_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            var editInvitationDto = await CreateValidEditInvitationDto();

            await InvitationsControllerTestsHelper.EditInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                38934,
                editInvitationDto,
                HttpStatusCode.BadRequest,
                "IPO with this ID does not exist!");
        }
        #endregion

        #region Sign 
        [TestMethod]
        public async Task SignPunchOut_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Anonymous,
                TestFactory.PlantWithoutAccess,
                9999,
                _sigurdSigner.Id,
                _sigurdSigner.RowVersion,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task SignPunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                _sigurdSigner.Id,
                _sigurdSigner.RowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Signer,
                TestFactory.UnknownPlant,
                9999,
                _sigurdSigner.Id,
                _sigurdSigner.RowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task SignPunchOut_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                _sigurdSigner.Id,
                _sigurdSigner.RowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task SignPunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                _sigurdSigner.Id,
                _sigurdSigner.RowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task SignPunchOut_AsSigner_ShouldReturnBadRequest_WhenUnknownInvitationId() 
            => await InvitationsControllerTestsHelper.SignPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                38934,
                _sigurdSigner.Id,
                _sigurdSigner.RowVersion,
                HttpStatusCode.BadRequest);
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
        public async Task CompletePunchOut_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task CompletePunchOut_AsCompleter_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Completer,
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
        public async Task CompletePunchOut_AsPlanner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CompletePunchOut_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CompletePunchOut_AsAccepter_ShouldReturnBadRequest_WhenInvalidParticipant()
        {
            var invitationToAcceptId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "TitleForComplete",
                "DescriptionForComplete",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null);

            var validInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var inValidParticipantForAccepting = validInvitation
                .Participants.Single(p => p.Organization == Organization.ConstructionCompany).Person;

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Accepter,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                new CompletePunchOutDto
                {
                    InvitationRowVersion = validInvitation.RowVersion,
                    ParticipantRowVersion = inValidParticipantForAccepting.Person.RowVersion,
                    Participants = new List<ParticipantToChangeDto>()
                },
                HttpStatusCode.BadRequest,
                "Person signing is not the contractor assigned to complete this IPO");
        }

        [TestMethod]
        public async Task CompletePunchOut_AsCompleter_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            var validInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);
            var validParticipantForCompleting = _participantsForSigning
                .Single(p => p.Organization == Organization.Contractor).Person;

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                38934,
                new CompletePunchOutDto
                {
                    InvitationRowVersion = validInvitation.RowVersion,
                    ParticipantRowVersion = validParticipantForCompleting.RowVersion,
                    Participants = new List<ParticipantToChangeDto>()
                },
                HttpStatusCode.BadRequest);
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
        public async Task UnCompletePunchOut_AsCompleter_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Completer,
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
        public async Task UnCompletePunchOut_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new UnCompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UnCompletePunchOut_AsCompleter_ShouldReturnBadRequest_WhenUnknownInvitationId() 
            => await InvitationsControllerTestsHelper.UnCompletePunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                38934,
                new UnCompletePunchOutDto
                {
                    InvitationRowVersion = null,
                    ParticipantRowVersion = null,
                },
                HttpStatusCode.BadRequest,
                "IPO with this ID does not exist!");

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
        public async Task AcceptPunchOut_AsAccepter_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Accepter,
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
        public async Task AcceptPunchOut_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new AcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task AcceptPunchOut_AsCompleter_ShouldReturnBadRequest_WhenInvalidParticipant()
        {
            var invitationToAcceptId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "TitleForAccept",
                "DescriptionForAccept",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participantsForSigning,
                _mcPkgScope,
                null);

            var validInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var inValidParticipantForAccepting = validInvitation
                .Participants.Single(p => p.Organization == Organization.Contractor).Person;

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                new CompletePunchOutDto
                {
                    InvitationRowVersion = validInvitation.RowVersion,
                    ParticipantRowVersion = inValidParticipantForAccepting.Person.RowVersion,
                    Participants = new List<ParticipantToChangeDto>()
                });

            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                new AcceptPunchOutDto
                {
                    InvitationRowVersion = validInvitation.RowVersion,
                    ParticipantRowVersion = inValidParticipantForAccepting.Person.RowVersion,
                    Participants = new List<ParticipantToUpdateNoteDto>()
                },
                HttpStatusCode.BadRequest,
                "Person signing is not the construction company assigned to accept this IPO");
        }

        [TestMethod]
        public async Task AcceptPunchOut_AsAccepter_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            var validInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);

            var validParticipantForAccepting = _participantsForSigning
                .Single(p => p.Organization == Organization.ConstructionCompany).Person;

            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Accepter,
                TestFactory.PlantWithAccess,
                38934,
                new AcceptPunchOutDto
                {
                    InvitationRowVersion = validInvitation.RowVersion,
                    ParticipantRowVersion = validParticipantForAccepting.RowVersion,
                    Participants = new List<ParticipantToUpdateNoteDto>()
                },
                HttpStatusCode.BadRequest,
                "IPO with this ID does not exist!");
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
        public async Task UnAcceptPunchOut_AsAccepter_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Accepter,
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
        public async Task UnAcceptPunchOut_AsSigner_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new UnAcceptPunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task UnAcceptPunchOut_AsAccepter_ShouldReturnBadRequest_WhenUnknownInvitationId() 
            => await InvitationsControllerTestsHelper.UnAcceptPunchOutAsync(
                UserType.Accepter,
                TestFactory.PlantWithAccess,
                38934,
                new UnAcceptPunchOutDto
                {
                    InvitationRowVersion = null,
                    ParticipantRowVersion = null,
                },
                HttpStatusCode.BadRequest,
                "IPO with this ID does not exist!");

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
        public async Task CancelPunchOut_AsCompleter_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Completer,
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
            => await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                9999,
                new CompletePunchOutDto(),
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task CancelPunchOut_AsCompleter_ShouldReturnBadRequest_WhenUnknownInvitationId() 
            => await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Completer,
                TestFactory.PlantWithAccess,
                38934,
                new CancelPunchOutDto(),
                HttpStatusCode.BadRequest);

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
        public async Task ChangeAttendedStatusOnParticipants_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
        {
            var validParticipantForUpdating = _participantsForSigning
                .Single(p => p.Organization == Organization.Contractor).Person;

            if (validParticipantForUpdating.Id != null)
            {
                await InvitationsControllerTestsHelper.ChangeAttendedStatusOnParticipantsAsync(
                    UserType.Planner,
                    TestFactory.PlantWithAccess,
                    38934,
                    new[]
                    {
                        new ParticipantToChangeDto
                        {
                            Id = (int)validParticipantForUpdating.Id,
                            Attended = true,
                            Note = "New note",
                            RowVersion = validParticipantForUpdating.RowVersion
                        }
                    },
                    HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region UploadAttachment
        [TestMethod]
        public async Task UploadAttachment_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Anonymous,
                TestFactory.PlantWithAccess,
                9999,
                FileToBeUploaded,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task UploadAttachment_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                9999,
                FileToBeUploaded,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.UnknownPlant,
                9999,
                FileToBeUploaded,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task UploadAttachment_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                9999,
                FileToBeUploaded,
                HttpStatusCode.Forbidden);


        [TestMethod]
        public async Task UploadAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                123456,
                FileToBeUploaded,
                HttpStatusCode.BadRequest,
                "Invitation doesn't exist!");
        #endregion

        #region DeleteAttachment
        [TestMethod]
        public async Task DeleteAttachment_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                InitialInvitationId,
                _attachmentId,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task DeleteAttachment_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialInvitationId,
                _attachmentId,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task DeleteAttachment_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                _attachmentId,
                TestFactory.AValidRowVersion,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                123456,
                _attachmentId,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "Invitation doesn't exist!");

        [TestMethod]
        public async Task DeleteAttachment_AsPlanner_ShouldReturnBadRequest_WhenUnknownAttachmentId()
            => await InvitationsControllerTestsHelper.DeleteAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                123456,
                TestFactory.AValidRowVersion,
                HttpStatusCode.BadRequest,
                "Attachment doesn't exist!");
        #endregion

        #region GetAttachments
        [TestMethod]
        public async Task GetAttachments_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Anonymous, 
                TestFactory.UnknownPlant,
                InitialInvitationId,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetAttachments_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialInvitationId,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetAttachments_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
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
                InitialInvitationId,
                _attachmentId,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetAttachmentById_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialInvitationId,
                _attachmentId,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetAttachmentById_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                _attachmentId,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetAttachmentById_AsPlanner_ShouldReturnNotFound_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                12456,
                _attachmentId,
                HttpStatusCode.NotFound);

        [TestMethod]
        public async Task GetAttachmentById_AsPlanner_ShouldReturnNotFound_WhenUnknownAttachmentId()
            => await InvitationsControllerTestsHelper.GetAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                12456,
                _attachmentId,
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
                "IPO with this ID does not exist");
        #endregion

        #region GetHistory
        [TestMethod]
        public async Task GetHistory_AsAnonymous_ShouldReturnUnauthorized()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                InitialInvitationId,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetHistory_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                InitialInvitationId,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetHistory_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                InitialInvitationId,
                HttpStatusCode.Forbidden);

        [TestMethod]
        public async Task GetHistory_AsPlanner_ShouldReturnNotFound_WhenUnknownInvitationId()
            => await InvitationsControllerTestsHelper.GetHistoryAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                123456,
                HttpStatusCode.NotFound);
        #endregion

        #region Private methods
        private async Task<EditInvitationDto> CreateValidEditInvitationDto()
        {
            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                InitialInvitationId);

            invitation.Status = IpoStatus.Planned;

            var editInvitationDto = new EditInvitationDto
            {
                Title = invitation.Title,
                Description = invitation.Description,
                StartTime = _invitationStartTime,
                EndTime = _invitationEndTime,
                Location = invitation.Location,
                ProjectName = invitation.ProjectName,
                RowVersion = invitation.RowVersion,
                UpdatedParticipants = ConvertToParticipantDtoEdit(invitation.Participants),
                UpdatedCommPkgScope = null,
                UpdatedMcPkgScope = _mcPkgScope
            };

            return editInvitationDto;
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
        #endregion
    }
}
