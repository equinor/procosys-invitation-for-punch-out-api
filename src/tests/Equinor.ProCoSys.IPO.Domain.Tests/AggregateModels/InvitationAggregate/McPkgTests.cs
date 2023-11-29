using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.InvitationAggregate
{
    [TestClass]
    public class McPkgTests
    {
        private McPkg _dut;
        private const string TestPlant = "PlantA";
        private const string ProjectName = "ProjectName";
        private static readonly Guid ProjectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private const int ProjectId = 320;
        private const string CommPkgNo = "Comm1";
        private const string McPkgNo = "Mc1";
        private const string Description = "D1";
        private const string System = "1|2";
        private static readonly Guid McPkgGuid = new Guid("11111111-2222-2222-6666-333333333333");
        private readonly Project _project = new(TestPlant, ProjectName, $"Description of {ProjectName}", ProjectGuid);


        [TestInitialize]
        public void Setup()
        {
            _project.SetProtectedIdForTesting(ProjectId);
            _dut = new McPkg(TestPlant, _project, CommPkgNo, McPkgNo, Description, System, McPkgGuid, Guid.Empty);
        }

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(ProjectId, _dut.ProjectId);
            Assert.AreEqual(CommPkgNo, _dut.CommPkgNo);
            Assert.AreEqual(McPkgNo, _dut.McPkgNo);
            Assert.AreEqual(Description, _dut.Description);
            Assert.AreEqual(McPkgGuid, _dut.Guid);
            Assert.IsFalse(_dut.RfocAccepted);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNameNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, null, CommPkgNo, McPkgNo, Description, System, Guid.Empty, Guid.Empty)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCommPkgNoNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, _project, null, McPkgNo, Description, System, Guid.Empty, Guid.Empty)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenMcPkgNoNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, _project, CommPkgNo, null, Description, System, Guid.Empty, Guid.Empty)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new McPkg(TestPlant, _project, CommPkgNo, McPkgNo, Description, null, Guid.Empty, Guid.Empty)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemIsTooShort() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new McPkg(TestPlant, _project, CommPkgNo, McPkgNo, Description, "1|", Guid.Empty, Guid.Empty)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenSystemIsInvalid() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new McPkg(TestPlant, _project, CommPkgNo, McPkgNo, Description, "1234", Guid.Empty, Guid.Empty)
            );
    }
}
