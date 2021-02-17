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
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "test title",
                "criteria",
                true);

            // Assert
            Assert.IsTrue(id > 0);
            //todo: when get saved filters is complete we can get and assert
        }

        //todo: when get saved filter is complete
        //[TestMethod]
        //public async Task DeleteSavedFilter_AsViewer_ShouldDeleteFilter()
        //{
        //    var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
        //        UserType.Viewer,
        //        TestFactory.PlantWithAccess,
        //        "test title",
        //        "criteria",
        //        true);

        //    var savedFilter = getSavedFilter
        //    // Act
        //    await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
        //        UserType.Viewer,
        //        TestFactory.PlantWithAccess,
        //        id,
        //        savedFilter.rowVersion);

        //    // Assert
        //}
    }
}
