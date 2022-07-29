using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetOutstandingIpos;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetOutstandingIpos
{
    [TestClass]
    public class GetOutstandingIposQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IMeApiService> _meApiServiceMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private GetOutstandingIposForCurrentPersonQuery _query;
        private Person _person;
        private Participant _personParticipantContractor;
        private Participant _personParticipant2;
        private Participant _functionalRoleParticipantConstructionCompany;
        private Participant _personParticipantConstructionCompany;
        private Participant _personParticipantSupplier;
        private Participant _functionalRoleParticipantContractor;

        private Invitation _invitationWithPersonParticipantContractor;
        private Invitation _invitationWithFunctionalRoleParticipantConstructionCompany;
        private Invitation _cancelledInvitation;
        private Invitation _invitationWithPersonParticipantConstructionCompany;
        private Invitation _invitationWithFunctionalRoleParticipantContractor;
        private string _functionalRoleCode = "FR1";
        private const string _projectName = "TestProject";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            _query = new GetOutstandingIposForCurrentPersonQuery();

            _person = new Person(_currentUserOid, "test@email.com", "FirstName", "LastName", "UserName");
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                IList<string> pcsFunctionalRoleCodes = new List<string> { _functionalRoleCode };

                _meApiServiceMock = new Mock<IMeApiService>();
                _meApiServiceMock
                    .Setup(x => x.GetFunctionalRoleCodesAsync(TestPlant))
                    .Returns(Task.FromResult(pcsFunctionalRoleCodes));

                _invitationWithPersonParticipantContractor = new Invitation(
                    TestPlant,
                    _projectName,
                    "TestInvitation1",
                    "TestDescription1",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    new List<CommPkg>());

                _invitationWithFunctionalRoleParticipantConstructionCompany = new Invitation(
                    TestPlant,
                    _projectName,
                    "TestInvitation2",
                    "TestDescription2",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    new List<CommPkg>());

                _cancelledInvitation = new Invitation(
                    TestPlant,
                    _projectName,
                    "TestInvitation3",
                    "TestDescription3",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    new List<CommPkg>());

                _invitationWithPersonParticipantConstructionCompany = new Invitation(
                    TestPlant,
                    _projectName,
                    "TestInvitation4",
                    "TestDescription4",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    new List<CommPkg>());

                _invitationWithFunctionalRoleParticipantContractor = new Invitation(
                    TestPlant,
                    _projectName,
                    "TestInvitation5",
                    "TestDescription5",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
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

                _invitationWithPersonParticipantContractor.AddParticipant(_personParticipantContractor);
                _invitationWithPersonParticipantContractor.AddParticipant(_personParticipantSupplier);
                _invitationWithFunctionalRoleParticipantConstructionCompany.AddParticipant(_functionalRoleParticipantConstructionCompany);
                _cancelledInvitation.AddParticipant(_personParticipant2);
                _invitationWithPersonParticipantConstructionCompany.AddParticipant(_personParticipantConstructionCompany);
                _invitationWithFunctionalRoleParticipantContractor.AddParticipant(_functionalRoleParticipantContractor);

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

                _cancelledInvitation.CancelIpo(_person);

                context.Invitations.Add(_invitationWithPersonParticipantContractor);
                context.Invitations.Add(_invitationWithFunctionalRoleParticipantConstructionCompany);
                context.Invitations.Add(_cancelledInvitation);
                context.Invitations.Add(_invitationWithPersonParticipantConstructionCompany);
                context.Invitations.Add(_invitationWithFunctionalRoleParticipantContractor);

                context.SaveChangesAsync().Wait();
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(4, result.Data.Items.Count());
                var outstandingInvitationWithPersonParticipantContractor = result.Data.Items.ElementAt(0);
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                        outstandingInvitationWithPersonParticipantContractor.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                        outstandingInvitationWithPersonParticipantContractor.Description);
                Assert.AreEqual(Organization.Contractor,
                    outstandingInvitationWithPersonParticipantContractor.Organization);

                var outstandingInvitationWithFunctionalRoleParticipantConstructionCompany = result.Data.Items.ElementAt(1);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Id,
                        outstandingInvitationWithFunctionalRoleParticipantConstructionCompany.InvitationId);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Description,
                        outstandingInvitationWithFunctionalRoleParticipantConstructionCompany.Description);
                Assert.AreEqual(Organization.ConstructionCompany,
                    outstandingInvitationWithFunctionalRoleParticipantConstructionCompany.Organization);

                var outstandingInvitationWithPersonParticipantConstructionCompany = result.Data.Items.ElementAt(2);
                Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id,
                    outstandingInvitationWithPersonParticipantConstructionCompany.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description,
                    outstandingInvitationWithPersonParticipantConstructionCompany.Description);
                Assert.AreEqual(Organization.ConstructionCompany,
                    outstandingInvitationWithPersonParticipantConstructionCompany.Organization);

                var outstandingInvitationWithFunctionalRoleParticipantContractor = result.Data.Items.ElementAt(3);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                    outstandingInvitationWithFunctionalRoleParticipantContractor.InvitationId);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                    outstandingInvitationWithFunctionalRoleParticipantContractor.Description);
                Assert.AreEqual(Organization.Contractor,
                    outstandingInvitationWithPersonParticipantContractor.Organization);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems_WhenUserIsNotInAnyFunctionalRoles()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider, _meApiServiceMock.Object, _plantProvider);
                IList<string> emptyListOfFunctionalRoleCodes = new List<string>();
                _meApiServiceMock
                    .Setup(x => x.GetFunctionalRoleCodesAsync(TestPlant))
                    .Returns(Task.FromResult(emptyListOfFunctionalRoleCodes));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(2, result.Data.Items.Count());
                var firstOutstandingInvitation = result.Data.Items.ElementAt(0);
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Id, firstOutstandingInvitation.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Description, firstOutstandingInvitation.Description);
                var secondOutstandingInvitation = result.Data.Items.ElementAt(1);
                Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id, secondOutstandingInvitation.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description, secondOutstandingInvitation.Description);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult_WhenNoUnCancelledIpoExists()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitationWithPersonParticipant =
                    context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantContractor.Id);

                var invitationWithFunctionalRoleParticipant =
                    context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

                var invitationWithPersonParticipantContractor =
                    context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

                var invitationWithFunctionalRoleParticipantContractor =
                    context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantContractor.Id);

                context.Remove(invitationWithPersonParticipant);
                context.Remove(invitationWithFunctionalRoleParticipant);
                context.Remove(invitationWithPersonParticipantContractor);
                context.Remove(invitationWithFunctionalRoleParticipantContractor);

                context.SaveChangesAsync().Wait();
            }

            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                    _meApiServiceMock.Object, _plantProvider);

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(0, result.Data.Items.Count());
                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldNotCheckForPersonsFunctionalRoles_WhenNoInvitationsExist()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitationWithPersonParticipant =
                    context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantContractor.Id);

                var invitationWithFunctionalRoleParticipant =
                    context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

                var cancelledInvitation = context.Invitations.Single(i => i.Id == _cancelledInvitation.Id);

                var invitationWithPersonParticipantContractor = 
                    context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

                var invitationWithFunctionalRoleParticipantContractor =
                    context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantContractor.Id);

                context.Remove(invitationWithPersonParticipant);
                context.Remove(invitationWithFunctionalRoleParticipant);
                context.Remove(cancelledInvitation);
                context.Remove(invitationWithPersonParticipantContractor);
                context.Remove(invitationWithFunctionalRoleParticipantContractor);

                context.SaveChangesAsync().Wait();

                var existingUncancelledInvitations = context.Invitations.Count(i => i.Status != IpoStatus.Canceled);
                Assert.AreEqual(0, existingUncancelledInvitations);

                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                    _meApiServiceMock.Object, _plantProvider);

                await dut.Handle(_query, default);

                _meApiServiceMock.Verify(meApiService => meApiService.GetFunctionalRoleCodesAsync(TestPlant), Times.Never);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnIpoForConstructionCompanyPerson_AfterIpoHasBeenAccepted()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitationWithPersonParticipantContractor =
                    context.Invitations.Single(i => i.Id == _invitationWithPersonParticipantConstructionCompany.Id);

                invitationWithPersonParticipantContractor.AcceptIpo(_personParticipantConstructionCompany,
                    _personParticipantContractor.RowVersion.ConvertToString(), _person, DateTime.Now);

                context.SaveChangesAsync().Wait();
            }

            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                    _meApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Items.Count());
                var outstandingInvitationWithPersonParticipantContractor = result.Data.Items.First();
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                    outstandingInvitationWithPersonParticipantContractor.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                    outstandingInvitationWithPersonParticipantContractor.Description);

                var outstandingInvitationWithFunctionalRoleParticipant = result.Data.Items.ElementAt(1);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Id,
                    outstandingInvitationWithFunctionalRoleParticipant.InvitationId);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantConstructionCompany.Description,
                    outstandingInvitationWithFunctionalRoleParticipant.Description);

                var outstandingInvitationWithFunctionalRoleParticipantContractor = result.Data.Items.ElementAt(2);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                    outstandingInvitationWithFunctionalRoleParticipantContractor.InvitationId);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                    outstandingInvitationWithFunctionalRoleParticipantContractor.Description);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldNotReturnIpoForConstructionCompanyFunctionalRole_AfterIpoHasBeenAccepted()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitationWithFunctionalRoleParticipantConstructionCompany =
                    context.Invitations.Single(i => i.Id == _invitationWithFunctionalRoleParticipantConstructionCompany.Id);

                invitationWithFunctionalRoleParticipantConstructionCompany.AcceptIpo(_personParticipantConstructionCompany,
                    _personParticipantContractor.RowVersion.ConvertToString(), _person, DateTime.Now);

                context.SaveChangesAsync().Wait();
            }

            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProvider,
                    _meApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Items.Count());
                var outstandingInvitationWithPersonParticipantContractor = result.Data.Items.First();
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Id,
                    outstandingInvitationWithPersonParticipantContractor.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantContractor.Description,
                    outstandingInvitationWithPersonParticipantContractor.Description);

                var outstandingInvitation = result.Data.Items.ElementAt(1);
                Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Id,
                    outstandingInvitation.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipantConstructionCompany.Description,
                    outstandingInvitation.Description);

                var outstandingInvitationWithFunctionalRoleParticipantContractor = result.Data.Items.ElementAt(2);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Id,
                    outstandingInvitationWithFunctionalRoleParticipantContractor.InvitationId);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipantContractor.Description,
                    outstandingInvitationWithFunctionalRoleParticipantContractor.Description);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenUserNotExists()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _currentUserProviderMock = new Mock<ICurrentUserProvider>();
                _currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Throws(new Exception("Unable to determine current user"));
                var dut = new GetOutstandingIposForCurrentPersonQueryHandler(context, _currentUserProviderMock.Object,
                    _meApiServiceMock.Object, _plantProvider);

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(0, result.Data.Items.Count());
                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }
    }
}
