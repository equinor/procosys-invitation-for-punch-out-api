using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.PersonAggregate
{
    [TestClass]
    public class PersonTests
    {
        private Guid Oid = new Guid("11111111-1111-2222-2222-333333333333");
        private const string TestPlant = "PCS$PlantA";
        private const string ProjectName = "Project name";

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var p = new Person(Oid, "FirstName", "LastName", "UserName", "EmailAddress");

            Assert.AreEqual(Oid, p.Oid);
            Assert.AreEqual("FirstName", p.FirstName);
            Assert.AreEqual("LastName", p.LastName);
            Assert.AreEqual("UserName", p.UserName);
            Assert.AreEqual("EmailAddress", p.Email);
        }

        [TestMethod]
        public void GetDefaultFilter_ShouldGetDefaultFilterWhenExists()
        {
            var dut = new Person(Oid, "firstName", "lastName", "", "");

            var savedFilter = new SavedFilter(TestPlant, ProjectName, "title", "criteria")
            {
                DefaultFilter = true
            };

            dut.AddSavedFilter(savedFilter);

            // Act
            var result = dut.GetDefaultFilter(ProjectName);

            // Arrange
            Assert.AreEqual(savedFilter, result);
        }
    }
}
