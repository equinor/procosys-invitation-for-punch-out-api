using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetMcPkgsUnderCommPkgInProject
{
    [TestClass]
    public class GetMcPkgsByCommPkgNoInProjectQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IMcPkgApiService> _mcPkgApiServiceMock;
        private IList<ProCoSysMcPkg> _mainApiMcPkgs;
        private GetMcPkgsUnderCommPkgInProjectQuery _query;

        private readonly string _projectName = "Dummy project";
        private readonly string _commPkgNo = "Comm-1";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();
                _mainApiMcPkgs = new List<ProCoSysMcPkg>
                {
                    new ProCoSysMcPkg
                    {
                        Id = 1, McPkgNo = "McPkgNo1", Description = "Desc1", DisciplineCode = "A"
                    },
                    new ProCoSysMcPkg
                    {
                        Id = 2, McPkgNo = "McPkgNo2", Description = "Desc2", DisciplineCode = "A"
                    },
                    new ProCoSysMcPkg
                    {
                        Id = 3, McPkgNo = "McPkgNo3", Description = "Desc3", DisciplineCode = "B"
                    }
                };

                _mcPkgApiServiceMock
                    .Setup(x => x.GetMcPkgsByCommPkgNoAndProjectNameAsync(TestPlant, _projectName, _commPkgNo))
                    .Returns(Task.FromResult(_mainApiMcPkgs));

                _query = new GetMcPkgsUnderCommPkgInProjectQuery(_projectName, _commPkgNo);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetMcPkgsUnderCommPkgInProjectQueryHandler(_mcPkgApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetMcPkgsUnderCommPkgInProjectQueryHandler(_mcPkgApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var item1 = result.Data.ElementAt(0);
                var item2 = result.Data.ElementAt(1);
                var item3 = result.Data.ElementAt(2);
                AssertMcPkgData(_mainApiMcPkgs.Single(c => c.McPkgNo == item1.McPkgNo), item1);
                AssertMcPkgData(_mainApiMcPkgs.Single(t => t.McPkgNo == item2.McPkgNo), item2);
                AssertMcPkgData(_mainApiMcPkgs.Single(t => t.McPkgNo == item3.McPkgNo), item3);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenSearchReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetMcPkgsUnderCommPkgInProjectQueryHandler(_mcPkgApiServiceMock.Object, _plantProvider);
                _mcPkgApiServiceMock
                    .Setup(x => x.GetMcPkgsByCommPkgNoAndProjectNameAsync(TestPlant, _projectName, _commPkgNo))
                    .Returns(Task.FromResult<IList<ProCoSysMcPkg>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private void AssertMcPkgData(ProCoSysMcPkg PCSMcPkg, ProCoSysMcPkgDto mcPkgDto)
        {
            Assert.AreEqual(PCSMcPkg.Id, mcPkgDto.Id);
            Assert.AreEqual(PCSMcPkg.McPkgNo, mcPkgDto.McPkgNo);
            Assert.AreEqual(PCSMcPkg.Description, mcPkgDto.Description);
            Assert.AreEqual(PCSMcPkg.DisciplineCode, mcPkgDto.DisciplineCode);
        }
    }
}
