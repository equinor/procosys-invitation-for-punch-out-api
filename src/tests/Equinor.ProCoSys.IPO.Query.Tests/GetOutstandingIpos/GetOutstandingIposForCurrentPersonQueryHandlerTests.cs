using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs;
using Equinor.ProCoSys.IPO.Query.GetOutstandingIpos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetOutstandingIpos
{
    [TestClass]
    public class GetOutstandingIposForCurrentPersonQueryHandlerTests : GetOutstandingIposForCurrentPersonQueryHandlerTestsBase
    {
        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            await AddAllInvitations(_dbContextOptions);
            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            await AddAllInvitations(_dbContextOptions);

            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            Assert.AreEqual(6, result.Data.Items.Count());
            var invitationWithPersonContractor = result.Data.Items.ElementAt(0);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                invitationWithPersonContractor.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                invitationWithPersonContractor.Description);
            Assert.AreEqual(Organization.Contractor,
                invitationWithPersonContractor.Organization);

            var invitationWithFrCC = result.Data.Items.ElementAt(1);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Id,
                invitationWithFrCC.InvitationId);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Description,
                invitationWithFrCC.Description);
            Assert.AreEqual(Organization.ConstructionCompany,
                invitationWithFrCC.Organization);

            var invitationWithPersonCC = result.Data.Items.ElementAt(2);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id,
                invitationWithPersonCC.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description,
                invitationWithPersonCC.Description);
            Assert.AreEqual(Organization.ConstructionCompany,
                invitationWithPersonCC.Organization);

            var invitationWithFrContractor = result.Data.Items.ElementAt(3);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                invitationWithFrContractor.InvitationId);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                invitationWithFrContractor.Description);
            Assert.AreEqual(Organization.Contractor,
                invitationWithPersonContractor.Organization);

            var acceptedInvitationWithOperation = result.Data.Items.ElementAt(4);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Id,
                acceptedInvitationWithOperation.InvitationId);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Description,
                acceptedInvitationWithOperation.Description);
            Assert.AreEqual(Organization.Operation,
                acceptedInvitationWithOperation.Organization);

            var invitationWithNotClosedProject = result.Data.Items.Last();
            Assert.AreEqual(_invitationForNotClosedProject.Id,
                invitationWithNotClosedProject.InvitationId);
            Assert.AreEqual(_invitationForNotClosedProject.Description,
                invitationWithNotClosedProject.Description);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems_WhenUserIsNotInAnyFunctionalRoles()
        {
            await AddAllInvitations(_dbContextOptions);

            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            IList<string> emptyListOfFunctionalRoleCodes = new List<string>();
            _meApiServiceMock
                .Setup(x => x.GetFunctionalRoleCodesAsync(TestPlant))
                .Returns(Task.FromResult(emptyListOfFunctionalRoleCodes));

            var result = await dut.Handle(_query, default);

            Assert.AreEqual(4, result.Data.Items.Count());
            var firstOutstandingInvitation = result.Data.Items.ElementAt(0);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Id, firstOutstandingInvitation.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Description, firstOutstandingInvitation.Description);
            var secondOutstandingInvitation = result.Data.Items.ElementAt(1);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id, secondOutstandingInvitation.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description, secondOutstandingInvitation.Description);
            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems_WhenUserIsInFunctionalRoles()
        {
            await AddAllInvitations(_dbContextOptions);

            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            IList<string> listOfFunctionalRoleCodes = new List<string> { "FR2", _functionalRoleCode };
            _meApiServiceMock
                .Setup(x => x.GetFunctionalRoleCodesAsync(TestPlant))
                .Returns(Task.FromResult(listOfFunctionalRoleCodes));

            var result = await dut.Handle(_query, default);

            Assert.AreEqual(6, result.Data.Items.Count());
            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult_WhenNoUnCancelledIpoExists()
        {
            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            var result = await dut.Handle(_query, default);

            Assert.AreEqual(0, result.Data.Items.Count());
            Assert.AreEqual(ResultType.Ok, result.ResultType);
            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldCheckForPersonsFunctionalRoles_WhenNoInvitationsExist()
        {
            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            await dut.Handle(_query, default);

            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnIpoForConstructionCompanyPerson_AfterIpoHasBeenAccepted()
        {
            await AddAllInvitations(_dbContextOptions);

            await using var context = CreateDbContext(_dbContextOptions);

            var invitationWithPersonCC =
                context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

            await AcceptIpo(context, invitationWithPersonCC, _personParticipantConstructionCompany, _person, DateTime.UtcNow);

            context.SaveChangesAsync().Wait();
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            Assert.AreEqual(5, result.Data.Items.Count());
            var invitationWithPersonContractor = result.Data.Items.First();
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                invitationWithPersonContractor.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                invitationWithPersonContractor.Description);

            var invitationWithFrCC = result.Data.Items.ElementAt(1);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Id,
                invitationWithFrCC.InvitationId);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Description,
                invitationWithFrCC.Description);

            var invitationWithFrContractor = result.Data.Items.ElementAt(2);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                invitationWithFrContractor.InvitationId);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                invitationWithFrContractor.Description);

            var acceptedInvitationWithOperation = result.Data.Items.ElementAt(3);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Id,
                acceptedInvitationWithOperation.InvitationId);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Description,
                acceptedInvitationWithOperation.Description);

            var invitationWithNotClosedProject = result.Data.Items.Last();
            Assert.AreEqual(_invitationForNotClosedProject.Id,
                invitationWithNotClosedProject.InvitationId);
            Assert.AreEqual(_invitationForNotClosedProject.Description,
                invitationWithNotClosedProject.Description);

            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnIpoForConstructionCompanyFunctionalRole_AfterIpoHasBeenAccepted()
        {
            var rowVersion = "AAAAAAAAAAA=";

            await AddAllInvitations(_dbContextOptions);

            await using var context = CreateDbContext(_dbContextOptions);
            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            await AcceptIpo(context, invitationWithFrCC, _personParticipantConstructionCompany, _person, DateTime.UtcNow);

            context.SaveChangesAsync().Wait();

            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            Assert.AreEqual(5, result.Data.Items.Count());
            var invitationWithPersonContractor = result.Data.Items.First();
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                invitationWithPersonContractor.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                invitationWithPersonContractor.Description);

            var invitationWithPersonCC = result.Data.Items.ElementAt(1);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id,
                invitationWithPersonCC.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description,
                invitationWithPersonCC.Description);

            var invitationWithFrContractor = result.Data.Items.ElementAt(2);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                invitationWithFrContractor.InvitationId);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                invitationWithFrContractor.Description);

            var acceptedInvitationWithOperation = result.Data.Items.ElementAt(3);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Id,
                acceptedInvitationWithOperation.InvitationId);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Description,
                acceptedInvitationWithOperation.Description);

            var invitationWithNotClosedProject = result.Data.Items.Last();
            Assert.AreEqual(_invitationForNotClosedProject.Id,
                invitationWithNotClosedProject.InvitationId);
            Assert.AreEqual(_invitationForNotClosedProject.Description,
                invitationWithNotClosedProject.Description);

            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

      

        [TestMethod]
        public async Task Handle_ShouldNotReturnIpoForConstructionCompanyFunctionalRole_AfterIpoScopeHasBeenHandedOver()
        {
            // Arrange

            await AddAllInvitations(_dbContextOptions);

            await using var context = CreateDbContext(_dbContextOptions);
            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            invitationWithFrCC.ScopeHandedOver();

            context.SaveChangesAsync().Wait();

            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            // Act
            var result = await dut.Handle(_query, default);

            // Assert
            Assert.AreEqual(5, result.Data.Items.Count());
            var invitationWithPersonContractor = result.Data.Items.First();
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                invitationWithPersonContractor.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                invitationWithPersonContractor.Description);

            var invitationWithPersonCC = result.Data.Items.ElementAt(1);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id,
                invitationWithPersonCC.InvitationId);
            Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description,
                invitationWithPersonCC.Description);

            var invitationWithFrContractor = result.Data.Items.ElementAt(2);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                invitationWithFrContractor.InvitationId);
            Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                invitationWithFrContractor.Description);

            var acceptedInvitationWithOperation = result.Data.Items.ElementAt(3);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Id,
                acceptedInvitationWithOperation.InvitationId);
            Assert.AreEqual(_acceptedInvitationWithOperationPerson.Description,
                acceptedInvitationWithOperation.Description);

            var invitationWithNotClosedProject = result.Data.Items.Last();
            Assert.AreEqual(_invitationForNotClosedProject.Id,
                invitationWithNotClosedProject.InvitationId);
            Assert.AreEqual(_invitationForNotClosedProject.Description,
                invitationWithNotClosedProject.Description);

            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenUserNotExists()
        {
            await AddAllInvitations(_dbContextOptions);

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Throws(new Exception("Unable to determine current user"));
            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProviderMock.Object,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            var result = await dut.Handle(_query, default);

            Assert.AreEqual(0, result.Data.Items.Count());
            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnInvitation_WhenProjectIsClosed()
        {
            await AddAllInvitations(_dbContextOptions);
            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            var isClosedProjectInvitationInResultSet = result.Data.Items.Any(x => x.Description.Equals(_closedProjectInvitationDescription));
            Assert.IsFalse(isClosedProjectInvitationInResultSet);
            Assert.AreEqual(6, result.Data.Items.Count());
        }

        [TestMethod]
        public async Task Handle_ShouldReturnInvitation_WhenProjectIsNotClosed()
        {
            await AddAllInvitations(_dbContextOptions);
            await using var context = CreateDbContext(_dbContextOptions);
            var repository = new OutstandingIpoRepository(context);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(repository, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            var isNotClosedProjectInvitationInResultSet = result.Data.Items.Any(x => x.Description.Equals(_notClosedProjectInvitationDescription));
            Assert.IsTrue(isNotClosedProjectInvitationInResultSet);
        }
    }
}
