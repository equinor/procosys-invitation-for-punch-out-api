using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.PersonAggregate
{
    [TestClass]
    public class SavedFilterTests
    {
        private const string TestPlant = "PCS$PlantA";
        private const string ProjectName = "Project name";
        private const string Title = "title";
        private const string Criteria = "criteria";
        private bool DefaultFilterValue = true;
        private SavedFilter _dut;

        [TestInitialize]
        public void Setup() =>
            _dut = new SavedFilter(TestPlant, ProjectName, Title, Criteria)
            {
                DefaultFilter = DefaultFilterValue
            };

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(ProjectName, _dut.ProjectName);
            Assert.AreEqual(Title, _dut.Title);
            Assert.AreEqual(Criteria, _dut.Criteria);
            Assert.AreEqual(DefaultFilterValue, _dut.DefaultFilter);
        }


        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNameNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new SavedFilter(TestPlant, null, Title, Criteria)
                {
                    DefaultFilter = DefaultFilterValue
                }
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenTitleNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new SavedFilter(TestPlant, ProjectName, null, Criteria)
                {
                    DefaultFilter = DefaultFilterValue
                }
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCriteriaNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new SavedFilter(TestPlant, ProjectName, Title, null)
                {
                    DefaultFilter = DefaultFilterValue
                }
            );
    }
}
