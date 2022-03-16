using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;
using Moq;

namespace Equinor.ProCoSys.IPO.Infrastructure.Tests.Repositories
{
    [TestClass]
    public class HistoryRepositoryTests : RepositoryTestBase
    {
        private List<History> _history;
        private Mock<DbSet<History>> _dbHistorySetMock;
        private Guid _guid1 = new Guid("11111111-2222-2222-2222-333333333333");
        private Guid _guid2 = new Guid("11111111-2222-2222-2222-333333333334");
        private History _createHistory1;
        private History _completeHistory1;
        private History _createHistory2;
        private History _cancelHistory2;

        private HistoryRepository _dut;

        [TestInitialize]
        public void Setup()
        {
            _createHistory1 = new History(TestPlant, "create first", _guid1, EventType.IpoCreated);
            _completeHistory1 = new History(TestPlant, "completed first", _guid1, EventType.IpoCompleted);
            _createHistory2 = new History(TestPlant, "create second", _guid2, EventType.IpoCreated);
            _cancelHistory2 = new History(TestPlant, "canceled second", _guid2, EventType.IpoCanceled);

            _history = new List<History>
            {
                _createHistory1,
                _completeHistory1,
                _createHistory2,
                _cancelHistory2
            };

            _dbHistorySetMock = _history.AsQueryable().BuildMockDbSet();
            ContextHelper
                .ContextMock
                .Setup(x => x.History)
                .Returns(_dbHistorySetMock.Object);

            _dut = new HistoryRepository(ContextHelper.ContextMock.Object);
        }


        [TestMethod]
        public void RemoveHistory_KnownHistory_ShouldRemoveHistory()
        {
            _dut.RemoveHistory(_completeHistory1);

            _dbHistorySetMock.Verify(s => s.Remove(_completeHistory1), Times.Once);
        }
    }
}
