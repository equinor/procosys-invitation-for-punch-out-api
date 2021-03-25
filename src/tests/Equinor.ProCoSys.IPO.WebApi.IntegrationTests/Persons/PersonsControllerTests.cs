 using System.Collections.Generic;
 using System.Linq;
 using System.Threading.Tasks;
 using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
 using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations;
 using Microsoft.VisualStudio.TestTools.UnitTesting;
 using DisciplineType = Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.DisciplineType;

 namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    [TestClass]
    public class PersonsControllerTests : PersonsControllerTestsBase
    {
        [TestMethod]
        public async Task CreateSavedFilter_AsViewer_ShouldSaveFilter()
        {
            // Act
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "test title",
                "criteria",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            
            var savedFilter = savedFilters.Find(sf => sf.Id == id);

            // Assert
            Assert.IsTrue(id > 0);
            Assert.IsTrue(savedFilters.Count > 0);
            Assert.IsNotNull(savedFilter);
            Assert.AreEqual(savedFilter.Title, "test title");
            Assert.AreEqual(savedFilter.Criteria, "criteria");
        }

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsViewer_ShouldGetFilters()
        {
            var id1 = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "filter1",
                "criteria",
                true);

            await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "filter2",
                "criteria",
                true);

            // Act
            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            // Assert
            var savedFilter = savedFilters.Single(sf => sf.Id == id1);
            Assert.IsTrue(savedFilters.Count >= 2);
            Assert.IsNotNull(savedFilter);
            Assert.AreEqual("filter1", savedFilter.Title);
            Assert.AreEqual("criteria", savedFilter.Criteria);
        }

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldUpdateFilter()
        {
            // Act
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "filter to update",
                "criteria to update",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var savedFilter = savedFilters.Single(sf => sf.Id == id);

            await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "new title",
                "new criteria",
                true,
                savedFilter.RowVersion,
                savedFilter.Id);

            var updatedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var updatedFilter = updatedFilters.Single(sf => sf.Id == id);

            // Assert
            Assert.IsNotNull(updatedFilter);
            Assert.AreNotEqual(updatedFilter.RowVersion, savedFilter.RowVersion);
            Assert.AreEqual("new title", updatedFilter.Title);
            Assert.AreEqual("new criteria", updatedFilter.Criteria);
        }

        [TestMethod]
        public async Task DeleteSavedFilter_AsViewer_ShouldDeleteFilter()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "test title 2",
                "criteria",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);

            var savedFilter = savedFilters.Single(f => f.Id == id);

            // Act
            await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                savedFilter.Id,
                savedFilter.RowVersion);

            // Assert
            savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);
            Assert.IsFalse(savedFilters.Exists(f => f.Id == id));
        }

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

            // Act
            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationId);

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

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationId,
                completePunchOutDto);

            var outstandingIpos = await PersonsControllerTestsHelper.GetOutstandingIposAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);

            // Assert
            var outstandingIpo = outstandingIpos.Items.Single(oi => oi.InvitationId == invitationId);
            Assert.IsTrue(outstandingIpos.MaxAvailable == 1);
            Assert.IsNotNull(outstandingIpo);
            Assert.AreEqual(invitationId, outstandingIpo.InvitationId);
            Assert.AreEqual(invitation.Description, outstandingIpo.Description);
        }
    }
}
