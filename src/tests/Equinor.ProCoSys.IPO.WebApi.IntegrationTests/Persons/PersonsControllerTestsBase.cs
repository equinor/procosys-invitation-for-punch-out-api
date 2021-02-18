using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    public class PersonsControllerTestsBase : TestBase
    {
        protected PersonHelper _vidarViewer;

        [TestInitialize]
        public void TestInitialize()
        {
            var viewerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Viewer);
            _vidarViewer = new PersonHelper(viewerUser.Profile.Oid, "Vidar", "Viewer", "VidarUserName",
                "vidar@viewer.com", 5, "AAAAAAAAASA=");

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
