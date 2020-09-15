using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.MainApi.Project;
using Equinor.ProCoSys.IPO.Query.GetProjectsInPlant;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetProjectsInPlant
{
    [TestClass]
    public class GetProjectsInPlantQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<IProjectApiService> _projectApiServiceMock;
        private IList<ProCoSysProject> _mainApiProjects;
        private GetProjectsInPlantQuery _query;

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _projectApiServiceMock = new Mock<IProjectApiService>();
                _mainApiProjects = new List<ProCoSysProject>
                {
                    new ProCoSysProject
                    {
                        Id = 1, Name = "ProjectName1", Description = "Desc1"
                    },
                    new ProCoSysProject
                    {
                        Id = 2, Name = "ProjectName2", Description = "Desc2"
                    },
                    new ProCoSysProject
                    {
                        Id = 3, Name = "ProjectName3", Description = "Desc3"
                    }
                };

                _projectApiServiceMock
                    .Setup(x => x.GetProjectsInPlantAsync(TestPlant))
                    .Returns(Task.FromResult(_mainApiProjects));

                _query = new GetProjectsInPlantQuery();
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetProjectsInPlantQueryHandler(_projectApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetProjectsInPlantQueryHandler(_projectApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var item1 = result.Data.ElementAt(0);
                var item2 = result.Data.ElementAt(1);
                var item3 = result.Data.ElementAt(2);
                AssertProjectData(_mainApiProjects.Single(p => p.Id == item1.Id), item1);
                AssertProjectData(_mainApiProjects.Single(p => p.Id == item2.Id), item2);
                AssertProjectData(_mainApiProjects.Single(p => p.Id == item3.Id), item3);
            }
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenReturnsNull()
        {
            using (new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new GetProjectsInPlantQueryHandler(_projectApiServiceMock.Object, _plantProvider);
                _projectApiServiceMock
                    .Setup(x => x.GetProjectsInPlantAsync(TestPlant))
                    .Returns(Task.FromResult<IList<ProCoSysProject>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        private void AssertProjectData(ProCoSysProject PCSProject, ProCoSysProjectDto projectDto)
        {
            Assert.AreEqual(PCSProject.Id, projectDto.Id);
            Assert.AreEqual(PCSProject.Name, projectDto.Name);
            Assert.AreEqual(PCSProject.Description, projectDto.Description);
        }
    }
}
