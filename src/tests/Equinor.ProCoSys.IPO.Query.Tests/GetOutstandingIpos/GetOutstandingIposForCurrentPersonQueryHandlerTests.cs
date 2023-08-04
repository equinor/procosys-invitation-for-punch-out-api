using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetOutstandingIpos;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetOutstandingIpos
{
    [TestClass]
    public class GetOutstandingIposForCurrentUserQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IMeApiService> _meApiServiceMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<ILogger<GetOutstandingIposForCurrentPersonQueryHandler>> _loggerMock;
        private GetOutstandingIposForCurrentPersonQuery _query;
        private Person _person;
        private Participant _personParticipantContractor;
        private Participant _personParticipant2;
        private Participant _functionalRoleParticipantConstructionCompany;
        private Participant _personParticipantConstructionCompany;
        private Participant _personParticipantSupplier;
        private Participant _personParticipantOperation;
        private Participant _functionalRoleParticipantContractor;
        private Participant _personParticipantClosedProject;
        private Participant _personParticipantNonClosedProject;

        private Invitation _acceptedInvitationWithOperationPerson;
        private Invitation _invitationWithPersonParticipantContractor;
        private Invitation _invitationWithFunctionalRoleParticipantConstructionCompany;
        private Invitation _cancelledInvitation;
        private Invitation _invitationWithPersonParticipantConstructionCompany;
        private Invitation _invitationWithFunctionalRoleParticipantContractor;
        private Invitation _invitationForClosedProject;
        private Invitation _invitationForNotClosedProject;
        private string _functionalRoleCode = "FR1";
        private const string _closedProjectInvitationTitle = "InvitationTitleForClosedProject";
        private const string _notClosedProjectInvitationTitle = "InvitationTitleForNOTClosedProject";
        private const string _closedProjectInvitationDescription = "InvitationDescriptionForClosedProject";
        private const string _notClosedProjectInvitationDescription = "InvitationDescriptionForNOTClosedProject"; 
        private Project _testProject;
        private Project _testProjectClosed;


        IPOContext CreateInMemoryContext()
        {
            var _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
        }

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            _loggerMock = new Mock<ILogger<GetOutstandingIposForCurrentPersonQueryHandler>>();

            _query = new GetOutstandingIposForCurrentPersonQuery();

            _person = new Person(_currentUserOid, "test@email.com", "FirstName", "LastName", "UserName");

            _testProject = new Project(TestPlant, "TestProject", "Description for TestProject");
            _testProjectClosed = new Project(TestPlant, "TestProject", "Description for TestProject"){IsClosed = true};
            _testProject.SetProtectedIdForTesting(1);
            _testProjectClosed.SetProtectedIdForTesting(2);

            using var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            IList<string> pcsFunctionalRoleCodes = new List<string> { _functionalRoleCode };

            _meApiServiceMock = new Mock<IMeApiService>();
            _meApiServiceMock
                .Setup(x => x.GetFunctionalRoleCodesAsync(TestPlant))
                .Returns(Task.FromResult(pcsFunctionalRoleCodes));

            _invitationWithPersonParticipantContractor = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation1",
                "TestDescription1",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _invitationWithFunctionalRoleParticipantConstructionCompany = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation2",
                "TestDescription2",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _cancelledInvitation = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation3",
                "TestDescription3",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _invitationWithPersonParticipantConstructionCompany = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation4",
                "TestDescription4",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _invitationWithFunctionalRoleParticipantContractor = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation5",
                "TestDescription5",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _acceptedInvitationWithOperationPerson = new Invitation(
                TestPlant,
                _testProject,
                "TestInvitation6",
                "TestDescription6",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _invitationForClosedProject = new Invitation(
                TestPlant,
                _testProjectClosed,
                _closedProjectInvitationTitle,
                _closedProjectInvitationDescription,
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProjectClosed, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _invitationForNotClosedProject = new Invitation(
                TestPlant,
                _testProject,
                _notClosedProjectInvitationTitle,
                _notClosedProjectInvitationDescription,
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null,
                new List<McPkg> { new McPkg(TestPlant, _testProject, "Comm", "Mc", "d", "1|2") },
                new List<CommPkg>());

            _functionalRoleParticipantConstructionCompany = new Participant(
                TestPlant,
                Organization.ConstructionCompany,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                1);
            _functionalRoleParticipantConstructionCompany.SetProtectedIdForTesting(1);

            _personParticipantContractor = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                0);

            var helperPerson = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                0);

            _personParticipantContractor.SetProtectedIdForTesting(2);

            _personParticipant2 = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                3);
            _personParticipant2.SetProtectedIdForTesting(3);

            _functionalRoleParticipantContractor = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                0);
            _functionalRoleParticipantContractor.SetProtectedIdForTesting(4);

            _personParticipantConstructionCompany = new Participant(
                TestPlant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                1);
            _personParticipantConstructionCompany.SetProtectedIdForTesting(5);

            _personParticipantSupplier = new Participant(
                TestPlant,
                Organization.Supplier,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                6);
            _personParticipantSupplier.SetProtectedIdForTesting(6);

            _personParticipantOperation = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                7);
            _personParticipantOperation.SetProtectedIdForTesting(7);

            _personParticipantClosedProject = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                7);
            _personParticipantClosedProject.SetProtectedIdForTesting(8);

            _personParticipantNonClosedProject = new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                null,
                _currentUserOid,
                7);
            _personParticipantNonClosedProject.SetProtectedIdForTesting(9);

            _invitationWithPersonParticipantContractor.AddParticipant(_personParticipantContractor);
            _invitationWithPersonParticipantContractor.AddParticipant(_personParticipantSupplier);
            _invitationWithFunctionalRoleParticipantConstructionCompany.AddParticipant(_functionalRoleParticipantConstructionCompany);
            _cancelledInvitation.AddParticipant(_personParticipant2);
            _invitationWithPersonParticipantConstructionCompany.AddParticipant(_personParticipantConstructionCompany);
            _invitationWithFunctionalRoleParticipantContractor.AddParticipant(_functionalRoleParticipantContractor);
            _acceptedInvitationWithOperationPerson.AddParticipant(_personParticipantOperation);
            _invitationForClosedProject.AddParticipant(_personParticipantClosedProject);
            _invitationForNotClosedProject.AddParticipant(_personParticipantNonClosedProject);

            _invitationWithPersonParticipantConstructionCompany.CompleteIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());

            _invitationWithFunctionalRoleParticipantConstructionCompany.CompleteIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());

            _acceptedInvitationWithOperationPerson.CompleteIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());
            _acceptedInvitationWithOperationPerson.AcceptIpo(
                helperPerson,
                helperPerson.RowVersion.ConvertToString(),
                _person,
                new DateTime());

            _cancelledInvitation.CancelIpo(_person);

            context.Projects.Add(_testProject);
            context.Projects.Add(_testProjectClosed);
            context.Invitations.Add(_invitationWithPersonParticipantContractor);
            context.Invitations.Add(_invitationWithFunctionalRoleParticipantConstructionCompany);
            context.Invitations.Add(_cancelledInvitation);
            context.Invitations.Add(_invitationWithPersonParticipantConstructionCompany);
            context.Invitations.Add(_invitationWithFunctionalRoleParticipantContractor);
            context.Invitations.Add(_acceptedInvitationWithOperationPerson);
            context.Invitations.Add(_invitationForClosedProject);
            context.Invitations.Add(_invitationForNotClosedProject);

            context.SaveChangesAsync().Wait();
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
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
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
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
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
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
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var invitationWithPersonContractor =
                context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantContractor.Id);

            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            var invitationWithPersonCC =
                context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

            var invitationWithFrContractor =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantContractor.Id);

            var acceptedInvitationWithOperation =
                context.Invitations.Single(i => i.Id == _acceptedInvitationWithOperationPerson.Id);

            var notClosedProjectInvitation =
                context.Invitations.Single(i => i.Id == _invitationForNotClosedProject.Id);

            var closedProjectInvitation =
                context.Invitations.Single(i => i.Id == _invitationForClosedProject.Id);

            context.Remove(invitationWithPersonContractor);
            context.Remove(invitationWithFrCC);
            context.Remove(invitationWithPersonCC);
            context.Remove(invitationWithFrContractor);
            context.Remove(acceptedInvitationWithOperation);
            context.Remove(notClosedProjectInvitation);
            context.Remove(closedProjectInvitation);

            context.SaveChangesAsync().Wait();

            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            var result = await dut.Handle(_query, default);

            Assert.AreEqual(0, result.Data.Items.Count());
            Assert.AreEqual(ResultType.Ok, result.ResultType);
            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ShouldNotCheckForPersonsFunctionalRoles_WhenNoInvitationsExist()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var invitationWithPersonContractor =
                context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantContractor.Id);

            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            var cancelledInvitation = context.Invitations.Single(i => i.Id == _cancelledInvitation.Id);

            var invitationWithPersonCC = 
                context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

            var invitationWithFrContractor =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantContractor.Id);

            var acceptedInvitationWithOperation =
                context.Invitations.Single(i => i.Id == _acceptedInvitationWithOperationPerson.Id);

            var notClosedProjectInvitation =
                context.Invitations.Single(i => i.Id == _invitationForNotClosedProject.Id);
                
            var closedProjectInvitation =
                context.Invitations.Single(i => i.Id == _invitationForClosedProject.Id);

            context.Remove(invitationWithPersonContractor);
            context.Remove(invitationWithFrCC);
            context.Remove(cancelledInvitation);
            context.Remove(invitationWithPersonCC);
            context.Remove(invitationWithFrContractor);
            context.Remove(acceptedInvitationWithOperation);
            context.Remove(notClosedProjectInvitation);
            context.Remove(closedProjectInvitation);

            context.SaveChangesAsync().Wait();

            var existingUncancelledInvitations = context.Invitations.Count(i => i.Status != IpoStatus.Canceled);
            Assert.AreEqual(0, existingUncancelledInvitations);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            await dut.Handle(_query, default);

            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ShouldNotCheckForPersonsFunctionalRoles_WhenNoFunctionalRolesOnIpos()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            var cancelledInvitation = context.Invitations.Single(i => i.Id == _cancelledInvitation.Id);

            var invitationWithFrContractor =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantContractor.Id);

            var notClosedProjectInvitation =
                context.Invitations.Single(i => i.Id == _invitationForNotClosedProject.Id);

            var closedProjectInvitation =
                context.Invitations.Single(i => i.Id == _invitationForClosedProject.Id);

            context.Remove(invitationWithFrCC);
            context.Remove(cancelledInvitation);
            context.Remove(invitationWithFrContractor);
            context.Remove(notClosedProjectInvitation);
            context.Remove(closedProjectInvitation);

            context.SaveChangesAsync().Wait();

            var existingUncancelledInvitations = context.Invitations.Count(i => i.Status != IpoStatus.Canceled);
            Assert.AreEqual(3, existingUncancelledInvitations);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            await dut.Handle(_query, default);

            _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Never);
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnIpoForConstructionCompanyPerson_AfterIpoHasBeenAccepted()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var invitationWithPersonCC =
                context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

            invitationWithPersonCC.AcceptIpo(_personParticipantConstructionCompany,
                _personParticipantConstructionCompany.RowVersion.ConvertToString(), _person, DateTime.Now);

            context.SaveChangesAsync().Wait();

            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
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
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            invitationWithFrCC.AcceptIpo(_personParticipantConstructionCompany,
                _personParticipantContractor.RowVersion.ConvertToString(), _person, DateTime.Now);

            context.SaveChangesAsync().Wait();

            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
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
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var invitationWithFrCC =
                context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

            invitationWithFrCC.ScopeHandedOver();

            context.SaveChangesAsync().Wait();

            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
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
            await using var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Throws(new Exception("Unable to determine current user"));
               
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProviderMock.Object,
                _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);

            var result = await dut.Handle(_query, default);

            Assert.AreEqual(0, result.Data.Items.Count());
            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnInvitation_WhenProjectIsClosed()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            var isClosedProjectInvitationInResultSet = result.Data.Items.Any(x => x.Description.Equals(_closedProjectInvitationDescription));
            Assert.IsFalse(isClosedProjectInvitationInResultSet);
            Assert.AreEqual(6, result.Data.Items.Count());
        }

        [TestMethod]
        public async Task Handle_ShouldReturnInvitation_WhenProjectIsNotClosed()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider, _loggerMock.Object);
            var result = await dut.Handle(_query, default);

            var isNotClosedProjectInvitationInResultSet = result.Data.Items.Any(x => x.Description.Equals(_notClosedProjectInvitationDescription));
            Assert.IsTrue(isNotClosedProjectInvitationInResultSet);
        }
    }
}
