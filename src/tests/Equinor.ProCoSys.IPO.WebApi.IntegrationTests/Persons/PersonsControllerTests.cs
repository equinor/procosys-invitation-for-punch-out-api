using System;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    [TestClass]
    public class PersonsControllerTests : PersonsControllerTestsBase
    {
        [TestMethod]
        public async Task CreateSavedFilterInProject_AsViewer_ShouldSaveFilter()
            => await CreateSavedFilterAndAssert(KnownTestData.ProjectName);

        [TestMethod]
        public async Task CreateSavedFilterWithoutProject_AsViewer_ShouldSaveFilter()
            => await CreateSavedFilterAndAssert(null);

        [TestMethod]
        public async Task GetSavedFiltersInProject_AsViewer_ShouldGetFilters()
        {
            var projectName = KnownTestData.ProjectName;
            var id1 = await PersonsControllerTestsHelper.CreateSavedFilterInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            var id2 = await PersonsControllerTestsHelper.CreateSavedFilterInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            // Act
            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName);

            // Assert
            Assert.IsTrue(savedFilters.Count >= 2);
            Assert.IsNotNull(savedFilters.SingleOrDefault(sf => sf.Id == id1));
            Assert.IsNotNull(savedFilters.SingleOrDefault(sf => sf.Id == id2));
        }

        [TestMethod]
        public async Task UpdateSavedFilter_AsViewer_ShouldUpdateFilter()
        {
            var projectName = KnownTestData.ProjectName;
            var id = await PersonsControllerTestsHelper.CreateSavedFilterInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName);

            var savedFilter = savedFilters.Single(sf => sf.Id == id);

            var newTitle = Guid.NewGuid().ToString();
            var newCriteria = Guid.NewGuid().ToString();
            
            // Act
            await PersonsControllerTestsHelper.UpdateSavedFilterAsync(
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
                projectName);

            var updatedFilter = updatedFilters.SingleOrDefault(sf => sf.Id == id);

            Assert.IsNotNull(updatedFilter);
            Assert.AreNotEqual(updatedFilter.RowVersion, savedFilter.RowVersion);
            Assert.AreEqual(newTitle, updatedFilter.Title);
            Assert.AreEqual(newCriteria, updatedFilter.Criteria);
        }

        [TestMethod]
        public async Task DeleteSavedFilter_AsViewer_ShouldDeleteFilter()
        {
            var id = await PersonsControllerTestsHelper.CreateSavedFilterInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                KnownTestData.ProjectName,
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

        private static async Task CreateSavedFilterAndAssert(string projectName)
        {
            // Act
            var title = Guid.NewGuid().ToString();
            var criteria = Guid.NewGuid().ToString();
            var id = await PersonsControllerTestsHelper.CreateSavedFilterInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName,
                title,
                criteria,
                true);

            var savedFilters = await PersonsControllerTestsHelper.GetSavedFiltersInProjectAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                projectName);

            var savedFilter = savedFilters.Find(sf => sf.Id == id);

            // Assert
            Assert.IsTrue(id > 0);
            Assert.IsTrue(savedFilters.Count > 0);
            Assert.IsNotNull(savedFilter);
            Assert.AreEqual(title, savedFilter.Title);
            Assert.AreEqual(criteria, savedFilter.Criteria);
        }
    }
}
