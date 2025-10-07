using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Equinor.ProCoSys.IPO.Query.GetProjectsInPlant;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetProjectsInPlant
{
    [TestClass]
    public class GetProjectsInPlantQueryHandlerTests
    {
        private readonly string _testPlant = "TestPlant";
        private Mock<IProjectApiForUsersService> _projectApiServiceMock;
        private Mock<IPlantProvider> _plantProviderMock;
        private IList<ProCoSysProject> _mainApiProjects;
        private GetProjectsInPlantQuery _query;
        private GetProjectsInPlantQueryHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock.Setup(x => x.Plant).Returns(_testPlant);

            _projectApiServiceMock = new Mock<IProjectApiForUsersService>();
            _mainApiProjects = new List<ProCoSysProject>
            {
                new()
                {
                    Id = 1, Name = "ProjectName1", Description = "Desc1"
                },
                new()
                {
                    Id = 2, Name = "ProjectName2", Description = "Desc2"
                },
                new()
                {
                    Id = 3, Name = "ProjectName3", Description = "Desc3"
                }
            };

            _projectApiServiceMock
                .Setup(x => x.GetProjectsInPlantAsync(_testPlant))
                .Returns(Task.FromResult(_mainApiProjects));

            _query = new GetProjectsInPlantQuery();
            _dut = new GetProjectsInPlantQueryHandler(_projectApiServiceMock.Object, _plantProviderMock.Object);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnOkResult()
        {
            var result = await _dut.Handle(_query, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnCorrectItems()
        {
            var result = await _dut.Handle(_query, default);

            Assert.AreEqual(3, result.Data.Count);
            var item1 = result.Data.ElementAt(0);
            var item2 = result.Data.ElementAt(1);
            var item3 = result.Data.ElementAt(2);
            AssertProjectData(_mainApiProjects.Single(p => p.Id == item1.Id), item1);
            AssertProjectData(_mainApiProjects.Single(p => p.Id == item2.Id), item2);
            AssertProjectData(_mainApiProjects.Single(p => p.Id == item3.Id), item3);
        }

        [TestMethod]
        public async Task Handle_ShouldReturnEmptyList_WhenReturnsNull()
        {
            _projectApiServiceMock
                .Setup(x => x.GetProjectsInPlantAsync(_testPlant))
                .Returns(Task.FromResult<IList<ProCoSysProject>>(null));

            var result = await _dut.Handle(_query, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
            Assert.AreEqual(0, result.Data.Count);
        }

        private void AssertProjectData(ProCoSysProject pcsProject, ProCoSysProjectDto projectDto)
        {
            Assert.AreEqual(pcsProject.Id, projectDto.Id);
            Assert.AreEqual(pcsProject.Name, projectDto.Name);
            Assert.AreEqual(pcsProject.Description, projectDto.Description);
        }
    }
}
