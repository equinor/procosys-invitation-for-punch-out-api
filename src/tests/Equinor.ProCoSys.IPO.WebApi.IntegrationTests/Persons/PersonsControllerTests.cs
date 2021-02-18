using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    [TestClass]
    public class PersonsControllerTests : PersonsControllerTestsBase
    {
        [TestMethod]
        public async Task CreateSavedFilter_AsViewer_ShouldSaveFilter()
        {
            // Act
            var id = await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "test title",
                "criteria",
                true);

            // Assert
            Assert.IsTrue(id > 0);
            //todo: when get saved filters is complete we can get and assert
        }

        //[TestMethod]
        //public async Task GetSavedFiltersInProject_AsViewer_ShouldGetFilters()
        //{
        //    // Act
        //    var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProject(
        //        UserType.Viewer,
        //        TestFactory.PlantWithAccess);

        //    // Assert
        //    Assert.IsTrue(true);
        //    //todo: when get saved filters is complete we can get and assert
        //}
    }
}
