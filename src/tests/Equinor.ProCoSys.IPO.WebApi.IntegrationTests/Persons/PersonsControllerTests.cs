using System;
using System.Linq;
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
            var title = Guid.NewGuid().ToString();
            var criteria = Guid.NewGuid().ToString();
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                title,
                criteria,
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            
            var savedFilter = savedFilters.Find(sf => sf.Id == id);

            // Assert
            Assert.IsTrue(id > 0);
            Assert.IsTrue(savedFilters.Count > 0);
            Assert.IsNotNull(savedFilter);
            Assert.AreEqual(title, savedFilter.Title);
            Assert.AreEqual(criteria, savedFilter.Criteria);
        }

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsViewer_ShouldGetFilters()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            // Act
            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            // Assert
            var savedFilter = savedFilters.Single(sf => sf.Id == id);
            Assert.IsTrue(savedFilters.Count >= 2);
            Assert.IsNotNull(savedFilter);
        }

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldUpdateFilter()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var savedFilter = savedFilters.Single(sf => sf.Id == id);

            var newTitle = Guid.NewGuid().ToString();
            var newCriteria = Guid.NewGuid().ToString();
            // Act
            await PersonsControllerTestsHelper.UpdateSavedFilter(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                savedFilter.Id,
                newTitle,
                newCriteria,
                true,
                savedFilter.RowVersion);

            // Assert
            var updatedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                null);

            var updatedFilter = updatedFilters.Single(sf => sf.Id == id);

            Assert.IsNotNull(updatedFilter);
            Assert.AreNotEqual(updatedFilter.RowVersion, savedFilter.RowVersion);
            Assert.AreEqual(newTitle, updatedFilter.Title);
            Assert.AreEqual(newCriteria, updatedFilter.Criteria);
        }

        [TestMethod]
        public async Task DeleteSavedFilter_AsViewer_ShouldDeleteFilter()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "test title 2",
                "criteria",
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);

            var savedFilter = savedFilters.Single(f => f.Id == id);

            // Act
            await PersonsControllerTestsHelper.DeleteSavedFilterAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                savedFilter.Id,
                savedFilter.RowVersion);

            // Assert
            savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                TestFactory.ProjectWithAccess);
            Assert.IsFalse(savedFilters.Exists(f => f.Id == id));
        }
    }
}
