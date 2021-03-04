using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.InvitationAggregate
{
    [TestClass]
    public class McPkgTests
    {
        private McPkg _dut;
        private const string TestPlant = "PlantA";
        private const string ProjectName = "ProjectName";
        private const string CommPkgNo = "Comm1";
        private const string McPkgNo = "Mc1";
        private const string Description = "D1";
        private const string System = "1|2";

        [TestInitialize]
        public void Setup() => _dut = new McPkg(TestPlant, ProjectName, CommPkgNo, McPkgNo, Description, System);

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(ProjectName, _dut.ProjectName);
            Assert.AreEqual(CommPkgNo, _dut.CommPkgNo);
            Assert.AreEqual(McPkgNo, _dut.McPkgNo);
            Assert.AreEqual(Description, _dut.Description);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNameNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, null, CommPkgNo, McPkgNo, Description, System)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCommPkgNoNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, ProjectName, null, McPkgNo, Description, System)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenMcPkgNoNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, ProjectName, CommPkgNo, null, Description, System)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, ProjectName, CommPkgNo, McPkgNo, Description, null)
            );
    }
}
