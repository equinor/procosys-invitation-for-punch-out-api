using System.Linq;
using System.Net;
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

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            
            var savedFilter = savedFilters.Find(sf => sf.Id == id);

            // Assert
            Assert.IsTrue(id > 0);
            Assert.IsTrue(savedFilters.Count > 0);
            Assert.IsNotNull(savedFilter);
            Assert.AreEqual(savedFilter.Title, "test title");
            Assert.AreEqual(savedFilter.Criteria, "criteria");
        }

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsViewer_ShouldGetFilters()
        {
            // Act
            var id1 = await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "filter1",
                "criteria",
                true);

            await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "filter2",
                "criteria",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var savedFilter = savedFilters.Single(sf => sf.Id == id1);

            // Assert
            Assert.IsTrue(savedFilters.Count >= 2);
            Assert.IsNotNull(savedFilter);
            Assert.AreEqual("filter1", savedFilter.Title);
            Assert.AreEqual("criteria", savedFilter.Criteria);
        }

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsViewer_ShouldGetNoFiltersWithUnknownProject()
        {
            // Act
            await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "test title3",
                "criteria",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "12345",
                HttpStatusCode.BadRequest);

            // Assert
            Assert.IsTrue(savedFilters.Count == 0);
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
