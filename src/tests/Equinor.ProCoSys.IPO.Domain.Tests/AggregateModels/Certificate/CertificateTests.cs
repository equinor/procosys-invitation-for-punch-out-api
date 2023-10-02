using System;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.Certificate
{
    [TestClass]
    public class CertificateTests
    {
        private Domain.AggregateModels.CertificateAggregate.Certificate _dut;
        private const string TestPlant = "PlantA";
        private const string ProjectName = "ProjectName";
        private const int ProjectId = 320;
        private Guid Guid = new Guid("00000000-0000-0000-0000-000000000001");
        private readonly Project _project = new(TestPlant, ProjectName, $"Description of {ProjectName}");


        [TestInitialize]
        public void Setup()
        {
            _project.SetProtectedIdForTesting(ProjectId);
            _dut = new Domain.AggregateModels.CertificateAggregate.Certificate(TestPlant, _project, Guid);
        }

        #region Constructor
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dut.Plant);
            Assert.AreEqual(ProjectId, _dut.ProjectId);
            Assert.AreEqual(Guid, _dut.PcsGuid);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Domain.AggregateModels.CertificateAggregate.Certificate(TestPlant, null, Guid)
            );
        #endregion

        #region AddCommPkgRelation
        [TestMethod]
        public void AddCommPkgRelation_ShouldAddRelation()
        {
            Assert.AreEqual(0, _dut.CertificateCommPkgs.Count);

            var commPkg = new CommPkg(TestPlant, _project, "123", "456", "OK", "1|2");
            _dut.AddCommPkgRelation(commPkg);

            Assert.AreEqual(1, _dut.CertificateCommPkgs.Count);
        }

        [TestMethod]
        public void AddCommPkgRelation_ShouldThrowException_WhenCommPkgIsNull() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                _dut.AddCommPkgRelation(null)
            );

        [TestMethod]
        public void AddCommPkgRelation_ShouldThrowException_WhenPlantsDoNotMatch() =>
            Assert.ThrowsException<ArgumentException>(() =>
                _dut.AddCommPkgRelation(new CommPkg("Plant B", new Project("Plant B", "new name", "test"), "123", "456", "OK", "1|2"))
            );
        #endregion

        #region AddMcPkgRelation
        [TestMethod]
        public void AddMcPkgRelation_ShouldAddRelation()
        {
            Assert.AreEqual(0, _dut.CertificateMcPkgs.Count);

            var mcPkg = new McPkg(TestPlant, _project, "123", "456", "OK", "1|2");
            _dut.AddMcPkgRelation(mcPkg);

            Assert.AreEqual(1, _dut.CertificateMcPkgs.Count);
        }

        [TestMethod]
        public void AddMcPkgRelation_ShouldThrowException_WhenMcPkgIsNull() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                _dut.AddMcPkgRelation(null)
            );

        [TestMethod]
        public void AddMcPkgRelation_ShouldThrowException_WhenPlantsDoNotMatch() =>
            Assert.ThrowsException<ArgumentException>(() =>
                _dut.AddMcPkgRelation(new McPkg("Plant B", new Project("Plant B", "new name", "test"), "123", "456", "OK", "1|2"))
            );
        #endregion
    }
}
