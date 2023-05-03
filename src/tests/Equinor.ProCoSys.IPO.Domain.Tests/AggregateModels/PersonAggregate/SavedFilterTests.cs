using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.PersonAggregate
{
    [TestClass]
    public class SavedFilterTests
    {
        private const string TestPlant = "PCS$PlantA";
        private const string ProjectName = "Project name";
        private const int ProjectId = 320;
        private const string Title = "title";
        private const string Criteria = "criteria";
        private bool DefaultFilterValue = true;
        private SavedFilter _dut;
        private readonly Project _project = new(TestPlant, ProjectName, $"Description of {ProjectName}");

        [TestInitialize]
        public void Setup()
        {
            _project.SetProtectedIdForTesting(ProjectId);
            _dut = new SavedFilter(TestPlant, _project, Title, Criteria) {DefaultFilter = DefaultFilterValue};
        }

        [TestMethod]
        public void Constructor_SetsProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(ProjectId, _dut.ProjectId.Value);
            Assert.AreEqual(Title, _dut.Title);
            Assert.AreEqual(Criteria, _dut.Criteria);
            Assert.AreEqual(DefaultFilterValue, _dut.DefaultFilter);
        }



        [TestMethod]
        public void Constructor_ShouldThrowException_WhenTitleNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new SavedFilter(TestPlant, _project, null, Criteria)
                {
                    DefaultFilter = DefaultFilterValue
                }
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCriteriaNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new SavedFilter(TestPlant, _project, Title, null)
                {
                    DefaultFilter = DefaultFilterValue
                }
            );
    }
}
