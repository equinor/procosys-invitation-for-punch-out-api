using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DisciplineType = Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.DisciplineType;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Me
{
    [TestClass]
    public class MeControllerTests : MeControllerTestsBase
    {
        [TestMethod]
        public async Task GetOutstandingIpos_AsSigner_ShouldGetOutstandingIpos()
        {
            //Arrange
            var invitationId = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                "InvitationTitle",
                "InvitationDescription",
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                _participants,
                _mcPkgScope,
                null);

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationId);

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
                        Id = completerParticipant.Id,
                        Note = "Some note about the punch out round or attendee",
                        RowVersion = completerParticipant.RowVersion,
                        Attended = true
                    }
                }
            };

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationId,
                completePunchOutDto);

            // Act
            var outstandingIpos = await MeControllerTestsHelper.GetOutstandingIposAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);

            // Assert
            var outstandingIpo = outstandingIpos.Items.Single(oi => oi.InvitationId == invitationId);
            Assert.IsNotNull(outstandingIpo);
            Assert.AreEqual(invitationId, outstandingIpo.InvitationId);
            Assert.AreEqual(invitation.Description, outstandingIpo.Description);
        }
    }
}
