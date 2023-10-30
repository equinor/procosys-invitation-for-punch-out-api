using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetHistory;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetHistory
{
    [TestClass]
    public class GetHistoryQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private Invitation _invitationWithHistory;
        private Invitation _invitationWithNoHistory;
        private History _historyCompleteIpo;
        private History _historyAcceptIpo;
        private GetHistoryQuery _query;

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            var project1 = new Project(TestPlant, "TestProject", $"Description of TestProject", ProjectGuid1);
            var project2 = new Project(TestPlant, "project", $"Description of project", ProjectGuid2);

            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var mcScope = new List<McPkg> {new McPkg(TestPlant, project1, "commno", "mcno", "d", "1|2")};

                _invitationWithNoHistory = new Invitation(
                    TestPlant,
                    project2,
                    "title",
                    "description",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null, 
                    mcScope,
                    null);

                _invitationWithHistory = new Invitation(
                    TestPlant,
                    project2,
                    "title 2",
                    "description",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    mcScope,
                    null);

                context.Invitations.Add(_invitationWithNoHistory);
                context.Invitations.Add(_invitationWithHistory);

                _historyCompleteIpo = new History(TestPlant, "D", _invitationWithHistory.Guid, EventType.IpoCompleted);
                _historyAcceptIpo = new History(TestPlant, "D1", _invitationWithHistory.Guid, EventType.IpoAccepted);

                context.History.Add(_historyCompleteIpo);
                context.History.Add(_historyAcceptIpo);

                context.SaveChangesAsync().Wait();

                _query = new GetHistoryQuery(_invitationWithHistory.Id);
            }
        }

        [TestMethod]
        public async Task HandleGetHistoryQuery_ShouldReturnOkResult()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                var dut = new GetHistoryQueryHandler(context);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task HandleGetHistoryQuery_ShouldReturnCorrectHistory_WhenTagHasHistory()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                var dut = new GetHistoryQueryHandler(context);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(2, result.Data.Count);
                AssertHistory(_historyCompleteIpo, result.Data.Single(t => t.EventType == EventType.IpoCompleted));
                AssertHistory(_historyAcceptIpo, result.Data.Single(t => t.EventType == EventType.IpoAccepted));
            }
        }

        [TestMethod]
        public async Task HandleGetHistoryQuery_ShouldReturnEmptyListOfHistory_WhenTagHasNoHistory()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                var dut = new GetHistoryQueryHandler(context);
                var result = await dut.Handle(new GetHistoryQuery(_invitationWithNoHistory.Id), default);

                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private void AssertHistory(History expected, HistoryDto actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.EventType, actual.EventType);
            Assert.AreEqual(expected.CreatedById, actual.CreatedBy.Id);
            Assert.AreEqual(expected.CreatedAtUtc, actual.CreatedAtUtc);
        }
    }
}
