using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    public class PersonsControllerTestsBase : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var project = new ProCoSysProject
            {
                Description = "description", Id = 1, IsClosed = false, Name = TestFactory.ProjectWithAccess
            };

            TestFactory.Instance
                .ProjectApiServiceMock
                .Setup(x => x.TryGetProjectAsync(
                    TestFactory.PlantWithAccess,
                    TestFactory.ProjectWithAccess))
                .Returns(Task.FromResult(project));
        }
    }
}
