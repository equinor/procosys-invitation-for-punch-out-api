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
        public async Task UpdateSavedFilter_AsViewer_ShouldUpdateFilter()
        {
            // Act
            var id = await PersonsControllerTestsHelper.CreateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "filter to update",
                "criteria to update",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var savedFilter = savedFilters.Single(sf => sf.Id == id);

            await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "new title",
                "new criteria",
                true,
                savedFilter.RowVersion,
                savedFilter.Id);

            var updatedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProject(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var updatedFilter = updatedFilters.Single(sf => sf.Id == id);

            // Assert
            Assert.IsNotNull(updatedFilter);
            Assert.AreNotEqual(updatedFilter.RowVersion, savedFilter.RowVersion);
            Assert.AreEqual("new title", updatedFilter.Title);
            Assert.AreEqual("new criteria", updatedFilter.Criteria);
        }
    }
}
