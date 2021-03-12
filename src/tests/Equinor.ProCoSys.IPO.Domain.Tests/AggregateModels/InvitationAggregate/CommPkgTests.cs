using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.InvitationAggregate
{
    [TestClass]
    public class CommPkgTests
    {
        private CommPkg _dut;
        private const string TestPlant = "PlantA";
        private const string ProjectName = "ProjectName";
        private const string CommPkgNo = "Comm1";
        private const string Description = "D1";
        private const string Status = "OK";
        private const string System = "1|2";

        [TestInitialize]
        public void Setup() => _dut = new CommPkg(TestPlant, ProjectName, CommPkgNo, Description, Status, System);

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(ProjectName, _dut.ProjectName);
            Assert.AreEqual(CommPkgNo, _dut.CommPkgNo);
            Assert.AreEqual(Status, _dut.Status);
            Assert.AreEqual(Description, _dut.Description);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNameNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new CommPkg(TestPlant, null, CommPkgNo, Description, Status, System)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCommPkgNoNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new CommPkg(TestPlant, ProjectName, null, Description, Status, System)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new CommPkg(TestPlant, ProjectName, CommPkgNo, Description, Status, null)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemIsTooShort() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new CommPkg(TestPlant, ProjectName, CommPkgNo, Description, Status, "1|")
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemIsInvalid() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new CommPkg(TestPlant, ProjectName, CommPkgNo, Description, Status, "1234")
            );
    }
}
