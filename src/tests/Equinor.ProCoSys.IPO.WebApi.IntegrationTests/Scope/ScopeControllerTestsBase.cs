using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Scope
{
    public class ScopeControllerTestsBase : TestBase
    {
        protected const string CommPkgNo1 = "COMMPKGNO1";
        protected const string CommPkgNo2 = "COMMPKGNO2";
        protected const string ProjectName1 = "PROJECTNAME1";
        protected const string ProjectName2 = "PROJECTNAME2";
        protected const string McPkgNo1 = "MCPKGNO1";
        protected const string McPkgNo2 = "MCPKGNO2";

        private IList<ProCoSysCommPkg> _commPkgs;
        private IList<ProCoSysProject> _projects;
        private IList<ProCoSysMcPkg> _mcPkgs;

        [TestInitialize]
        public void TestInitialize()
        {
            _commPkgs = new List<ProCoSysCommPkg>
            {
                new ProCoSysCommPkg
                {
                    CommPkgNo = CommPkgNo1,
                    CommStatus = "OK",
                    Description = "CommPkg1Description",
                    Id = 1,
                    SystemId = 2
                },
                new ProCoSysCommPkg
                {
                    CommPkgNo = CommPkgNo2,
                    CommStatus = "OS",
                    Description = "CommPkg2Description",
                    Id = 2,
                    SystemId = 2
                }
            };

            _projects = new List<ProCoSysProject>
            {
                new ProCoSysProject {Id = 1, Name = ProjectName1, Description = "Project1Description"},
                new ProCoSysProject {Id = 2, Name = ProjectName2, Description = "Project2Description"}
            };

            _mcPkgs = new List<ProCoSysMcPkg>
            {
                new ProCoSysMcPkg
                {
                    Id = 1,
                    CommPkgNo = CommPkgNo1,
                    McPkgNo = McPkgNo1,
                    Description = "McPkg1Description",
                    DisciplineCode = "A"
                },
                new ProCoSysMcPkg
                {
                    Id = 2,
                    CommPkgNo = CommPkgNo2,
                    McPkgNo = McPkgNo2,
                    Description = "McPkg2Description",
                    DisciplineCode = "B"
                }
            };

            TestFactory.Instance
                .CommPkgApiServiceMock
                .Setup(x => x.SearchCommPkgsByCommPkgNoAsync(
                    TestFactory.PlantWithAccess,
                    TestFactory.ProjectWithAccess,
                    "CommPkgNo"))
                .Returns(Task.FromResult(_commPkgs));

            TestFactory.Instance
                .ProjectApiServiceMock
                .Setup(x => x.GetProjectsInPlantAsync(
                    TestFactory.PlantWithAccess))
                .Returns(Task.FromResult(_projects));

            TestFactory.Instance
                .McPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByCommPkgNoAndProjectNameAsync(
                    TestFactory.PlantWithAccess,
                    TestFactory.ProjectWithAccess,
                    "CommPkgNo"))
                .Returns(Task.FromResult(_mcPkgs));
        }
    }
}
