using System;
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

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldUpdateFilter()
        {
            // Act
            // todo: get a filter id to update
            var rowVersion = await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "new title",
                "new criteria",
                true,
                "rowVersion");

            // todo: get filter again to verify update
            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(rowVersion));
            //todo: when get saved filters is complete we can get and assert
        }
    }
}
