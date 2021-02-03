using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Scope
{
    [TestClass]
    public class ScopeControllerTests : ScopeControllerTestsBase
    {
        [TestMethod]
        public async Task GetCommPkgsInProject_AsViewer_ShouldGetCommPkgsInProject()
        {
            // Act
            var commPkgSearchResult = await ScopeControllerTestsHelper.GetCommPkgsInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess,
                "CommPkgNo");

            // Assert
            var commPkg1 = commPkgSearchResult.CommPkgs.First();
            var commPkg2 = commPkgSearchResult.CommPkgs.Last();
            Assert.AreEqual(2, commPkgSearchResult.CommPkgs.Count);
            Assert.AreEqual(2, commPkgSearchResult.MaxAvailable);
            Assert.AreEqual(CommPkgNo1, commPkg1.CommPkgNo);
            Assert.AreEqual(CommPkgNo2, commPkg2.CommPkgNo);
        }

        [TestMethod]
        public async Task GetProjectsInPlant_AsViewer_ShouldGetProjectsInPlant()
        {
            // Act
            var projects = await ScopeControllerTestsHelper.GetProjectsInPlantAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess);

            // Assert
            var project1 = projects.First();
            var project2 = projects.Last();
            Assert.AreEqual(2, projects.Count);
            Assert.AreEqual(ProjectName1, project1.Name);
            Assert.AreEqual(ProjectName2, project2.Name);
        }

        [TestMethod]
        public async Task GetMcPkgsInProject_AsViewer_ShouldGetMcPkgsInProject()
        {
            // Act
            var mcPkgs = await ScopeControllerTestsHelper.GetMcPkgsInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess,
                "CommPkgNo");

            // Assert
            var mcPkg1 = mcPkgs.First();
            var mcPkg2 = mcPkgs.Last();
            Assert.AreEqual(2, mcPkgs.Count);
            Assert.AreEqual(McPkgNo1, mcPkg1.McPkgNo);
            Assert.AreEqual(McPkgNo2, mcPkg2.McPkgNo);
        }
    }
}
