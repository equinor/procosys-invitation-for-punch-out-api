using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetCommPkgsInProject
{
    [TestClass]
    public class SearchCommPkgsByCommPkgNoQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private Mock<ICommPkgApiService> _commPkgApiServiceMock;
        private IList<ProCoSysSearchCommPkg> _mainApiCommPkgs;
        private GetCommPkgsInProjectQuery _query;

        private readonly string _projectName = "Pname";
        private readonly string _commPkgStartsWith = "C";
        private readonly int _defaultPageSize = 10;
        private readonly int _defaultCurrentPage = 0;
        private readonly DateTime RfocAcceptedAt = new DateTime(2017, 11, 3);

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _commPkgApiServiceMock = new Mock<ICommPkgApiService>();
                _mainApiCommPkgs = new List<ProCoSysSearchCommPkg>
                {
                    new ProCoSysSearchCommPkg
                    {
                        Id = 1, CommPkgNo = "CommPkgNo1", Description = "Desc1", CommStatus = "PB", RfocAcceptedAt = RfocAcceptedAt
                    },
                    new ProCoSysSearchCommPkg
                    {
                        Id = 2, CommPkgNo = "CommPkgNo2", Description = "Desc2", CommStatus = "OK", RfocAcceptedAt = null
                    },
                    new ProCoSysSearchCommPkg
                    {
                        Id = 3, CommPkgNo = "CommPkgNo3", Description = "Desc3", CommStatus = "PA", RfocAcceptedAt = null
                    }
                };

                var result = new ProCoSysCommPkgSearchResult {MaxAvailable = 3, Items = _mainApiCommPkgs};

                _commPkgApiServiceMock
                    .Setup(x => x.SearchCommPkgsByCommPkgNoAsync(TestPlant, _projectName, _commPkgStartsWith, _defaultPageSize, _defaultCurrentPage))
                    .Returns(Task.FromResult(result));

                _query = new GetCommPkgsInProjectQuery(_projectName, _commPkgStartsWith, _defaultPageSize, _defaultCurrentPage);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetCommPkgsInProjectQueryHandler(_commPkgApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetCommPkgsInProjectQueryHandler(_commPkgApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.CommPkgs.Count);
                Assert.AreEqual(3, result.Data.MaxAvailable);
                var item1 = result.Data.CommPkgs.ElementAt(0);
                var item2 = result.Data.CommPkgs.ElementAt(1);
                var item3 = result.Data.CommPkgs.ElementAt(2);
                AssertCommPkgData(_mainApiCommPkgs.Single(c => c.CommPkgNo == item1.CommPkgNo), item1);
                AssertCommPkgData(_mainApiCommPkgs.Single(t => t.CommPkgNo == item2.CommPkgNo), item2);
                AssertCommPkgData(_mainApiCommPkgs.Single(t => t.CommPkgNo == item3.CommPkgNo), item3);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetCommPkgsInProjectQueryHandler(_commPkgApiServiceMock.Object, _plantProvider);
                _commPkgApiServiceMock
                    .Setup(x => x.SearchCommPkgsByCommPkgNoAsync(TestPlant, _projectName, _commPkgStartsWith, _defaultPageSize, _defaultCurrentPage))
                    .Returns(Task.FromResult(new ProCoSysCommPkgSearchResult
                    {
                        MaxAvailable = 0,
                        Items = null
                    }));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.CommPkgs.Count);
            }
        }

        private void AssertCommPkgData(ProCoSysSearchCommPkg pcsSearchCommPkg, ProCoSysCommPkgDto commPkgDto)
        {
            Assert.AreEqual(pcsSearchCommPkg.Id, commPkgDto.Id);
            Assert.AreEqual(pcsSearchCommPkg.CommPkgNo, commPkgDto.CommPkgNo);
            Assert.AreEqual(pcsSearchCommPkg.Description, commPkgDto.Description);
            Assert.AreEqual(pcsSearchCommPkg.CommStatus, commPkgDto.Status);
            Assert.AreEqual(pcsSearchCommPkg.RfocAcceptedAt, commPkgDto.RfocAcceptedAt);
        }
    }
}
