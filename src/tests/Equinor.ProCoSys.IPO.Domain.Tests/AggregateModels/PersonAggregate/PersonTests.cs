using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.PersonAggregate
{
    [TestClass]
    public class PersonTests
    {
        private Guid Oid = new Guid("11111111-1111-2222-2222-333333333333");
        private const string TestPlant = "PCS$PlantA";
        private const string ProjectName = "Project name";
        private Person _dut;
        private SavedFilter _savedDefaultFilter;
        private readonly Project _project = new(TestPlant, ProjectName, $"Description of {ProjectName}");

        [TestInitialize]
        public void Setup()
        {
            _dut = new Person(Oid, "FirstName", "LastName", "UserName", "EmailAddress");
            _savedDefaultFilter = new SavedFilter(TestPlant, _project, "title", "criteria")
            {
                DefaultFilter = true
            };
            _dut.AddSavedFilter(_savedDefaultFilter);
        }


        [TestMethod]
        public void Constructor_SetsProperties()
        {
            Assert.AreEqual(Oid, _dut.Oid);
            Assert.AreEqual("FirstName", _dut.FirstName);
            Assert.AreEqual("LastName", _dut.LastName);
            Assert.AreEqual("UserName", _dut.UserName);
            Assert.AreEqual("EmailAddress", _dut.Email);
        }

        [TestMethod]
        public void CreateSavedFilter_ShouldSaveFilter()
        {
            var savedFilter = new SavedFilter(TestPlant, _project, "titleNew", "criteria")
            {
                DefaultFilter = true
            };

            _dut.AddSavedFilter(savedFilter);

            // Act
            var result = _dut.SavedFilters.ToList().Last();

            // Arrange
            Assert.AreEqual(savedFilter, result);
        }

        [TestMethod]
        public void CreateSavedFilterWithoutProject_ShouldSaveFilter()
        {
            var savedFilter = new SavedFilter(TestPlant, null, "titleNew", "criteria")
            {
                DefaultFilter = true
            };

            _dut.AddSavedFilter(savedFilter);

            // Act
            var result = _dut.SavedFilters.ToList().Last();

            // Arrange
            Assert.AreEqual(savedFilter, result);
        }

        [TestMethod]
        public void GetDefaultFilter_ShouldGetDefaultFilterWhenExists()
        {
            var dut = new Person(Oid, "firstName", "lastName", "", "");

            var savedFilter = new SavedFilter(TestPlant, _project, "title", "criteria")
            {
                DefaultFilter = true
            };

            dut.AddSavedFilter(savedFilter);

            // Act
            var result = dut.GetDefaultFilter(_project);

            // Arrange
            Assert.AreEqual(savedFilter, result);
        }

        public void DeleteSavedFilter_ShouldSaveFilter()
        {
            var savedFilter = new SavedFilter(TestPlant, _project, "title", "criteria")
            {
                DefaultFilter = true
            };

            _dut.AddSavedFilter(savedFilter);

            // Act
            var result = _dut.SavedFilters.ToList().Single();

            // Arrange
            Assert.AreEqual(savedFilter, result);
        }
    }
}
