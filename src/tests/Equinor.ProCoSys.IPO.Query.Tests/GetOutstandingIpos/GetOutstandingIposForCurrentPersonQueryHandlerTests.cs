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
        private GetOutstandingIposForCurrentPersonQuery _query;
        private Person _person;

        private Invitation _invitationWithPersonParticipant;
        private Invitation _invitationWithFunctionalRoleParticipant;
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

                _invitationWithPersonParticipant = new Invitation(
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

                _invitationWithFunctionalRoleParticipant = new Invitation(
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

                var FunctionalRoleParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    _functionalRoleCode,
                    null,
                    null,
                    null,
                    null,
                    null,
                    1);
                FunctionalRoleParticipant.SetProtectedIdForTesting(1);

                var PersonParticipant = new Participant(
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
                PersonParticipant.SetProtectedIdForTesting(2);

                _invitationWithPersonParticipant.AddParticipant(PersonParticipant);
                _invitationWithFunctionalRoleParticipant.AddParticipant(FunctionalRoleParticipant);

                _invitationWithPersonParticipant.CompleteIpo(
                    PersonParticipant,
                    PersonParticipant.RowVersion.ConvertToString(),
                    _person,
                    new DateTime());

                _invitationWithFunctionalRoleParticipant.CompleteIpo(
                    FunctionalRoleParticipant,
                    FunctionalRoleParticipant.RowVersion.ConvertToString(),
                    _person,
                    new DateTime());

                context.Invitations.Add(_invitationWithPersonParticipant);
                context.Invitations.Add(_invitationWithFunctionalRoleParticipant);
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

                Assert.AreEqual(2, result.Data.Items.Count());
                var outstandingInvitationWithPersonParticipant = result.Data.Items.ElementAt(0);
                Assert.AreEqual(_invitationWithPersonParticipant.Id, outstandingInvitationWithPersonParticipant.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipant.Description, outstandingInvitationWithPersonParticipant.Description);
                var outstandingInvitationWithFunctionalRoleParticipant = result.Data.Items.ElementAt(1);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipant.Id, outstandingInvitationWithFunctionalRoleParticipant.InvitationId);
                Assert.AreEqual(_invitationWithFunctionalRoleParticipant.Description, outstandingInvitationWithFunctionalRoleParticipant.Description);
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

                Assert.AreEqual(1, result.Data.Items.Count());
                var outstandingInvitation = result.Data.Items.First();
                Assert.AreEqual(_invitationWithPersonParticipant.Id, outstandingInvitation.InvitationId);
                Assert.AreEqual(_invitationWithPersonParticipant.Description, outstandingInvitation.Description);
            }
        }
    }
}
