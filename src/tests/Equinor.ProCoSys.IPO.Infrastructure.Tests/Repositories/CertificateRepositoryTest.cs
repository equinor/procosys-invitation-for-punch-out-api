using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;
using Moq;

namespace Equinor.ProCoSys.IPO.Infrastructure.Tests.Repositories
{
    [TestClass]
    public class CertificateRepositoryTests : RepositoryTestBase
    {
        private Mock<DbSet<Certificate>> _dbCertificateSetMock;
        private Guid _knownGuid= new Guid("11111111-2222-2222-2222-333333333333");
        private Guid _unknownGuid = new Guid("11111111-2222-2222-2222-333333333334");

        private CertificateRepository _dut;

        [TestInitialize]
        public void Setup()
        {
            var testPlant = "TestPlant";
            var project = new Project("TestPlant", "title", "description", Guid.NewGuid());
            var certificate = new Certificate(testPlant, project, _knownGuid, true);
            var mcPkg = new McPkg(testPlant, project, "123", "456", "desc", "1|2",Guid.Empty, Guid.Empty);
            var commPkg = new CommPkg(testPlant, project, "123", "desc", "ok", "1|2", Guid.Empty);
            certificate.CertificateMcPkgs.Add(mcPkg);
            certificate.CertificateCommPkgs.Add(commPkg);

            var certificates = new List<Certificate>
            {
                certificate
            };

            _dbCertificateSetMock = certificates.AsQueryable().BuildMockDbSet();
            ContextHelper
                .ContextMock
                .Setup(x => x.Certificates)
                .Returns(_dbCertificateSetMock.Object);

            _dut = new CertificateRepository(ContextHelper.ContextMock.Object);
        }

        [TestMethod]
        public async Task GetCertificateByGuid_KnownCertificate_ShouldGetCertificate()
        {
            var certificate = await _dut.GetCertificateByGuid(_knownGuid);

            Assert.IsNotNull(certificate);
            Assert.AreEqual(_knownGuid, certificate.PcsGuid);
        }

        [TestMethod]
        public async Task GetCertificateByGuid_KnownCertificate_ShouldGetScope()
        {
            var certificate = await _dut.GetCertificateByGuid(_knownGuid);

            Assert.AreEqual(1, certificate.CertificateMcPkgs.Count);
            Assert.AreEqual(1, certificate.CertificateCommPkgs.Count);
        }

        [TestMethod]
        public async Task GetCertificateByGuid_UnknownCertificate_ShouldNotGetCertificate()
        {
            var certificate = await _dut.GetCertificateByGuid(_unknownGuid);

            Assert.IsNull(certificate);
        }
    }
}
