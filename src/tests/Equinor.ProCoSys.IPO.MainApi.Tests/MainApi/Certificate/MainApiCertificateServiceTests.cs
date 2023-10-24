using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.MainApi.Certificate
{
    [TestClass]
    public class MainApiCertificateServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IMainApiClient> _foreignApiClient;
        private MainApiCertificateService _dut;

        private PCSCertificateCommPkg _commPkg1;
        private PCSCertificateMcPkg _mcPkg1;
        private PCSCertificateCommPkgsModel _certificateCommPkgsModel;
        private PCSCertificateMcPkgsModel _certificateMcPkgsModel;

        private const string _plant = "PCS$TESTPLANT";

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<IMainApiClient>();

            _commPkg1 = new PCSCertificateCommPkg
            {
                CommPkgNo = "CommNo1"
            };
            var commPkg2 = new PCSCertificateCommPkg
            {
                CommPkgNo = "CommNo2"
            };
            var commPkg3 = new PCSCertificateCommPkg
            {
                CommPkgNo = "CommNo3"
            };
            _certificateCommPkgsModel = new PCSCertificateCommPkgsModel
            {
                CertificateIsAccepted = false,
                CommPkgs = new List<PCSCertificateCommPkg> {_commPkg1, commPkg2, commPkg3}
            };

            _mcPkg1 = new PCSCertificateMcPkg
            {
                McPkgNo = "McNo1",
                CommPkgNo = "CommNo1"
            };
            var mcPkg2 = new PCSCertificateMcPkg
            {
                McPkgNo = "McNo2",
                CommPkgNo = "CommNo1"
            };
            var mcPkg3 = new PCSCertificateMcPkg
            {
                McPkgNo = "McNo3",
                CommPkgNo = "CommNo3"
            };
            _certificateMcPkgsModel = new PCSCertificateMcPkgsModel
            {
                CertificateIsAccepted = false,
                McPkgs = new List<PCSCertificateMcPkg> { _mcPkg1, mcPkg2, mcPkg3 }
            };

            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<PCSCertificateCommPkgsModel>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(_certificateCommPkgsModel));

            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<PCSCertificateMcPkgsModel>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(_certificateMcPkgsModel));

            _dut = new MainApiCertificateService(_foreignApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task TryGetCertificateMcPkgsAsync_ShouldReturnCorrectNumberOfMcPkgs()
        {
            // Act
            var result = await _dut.TryGetCertificateMcPkgsAsync(_plant, new Guid());

            // Assert
            Assert.AreEqual(3, result.McPkgs.Count());
        }

        [TestMethod]
        public async Task TryGetCertificateMcPkgsAsync_ShouldReturnNull_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<PCSCertificateMcPkgsModel>(It.IsAny<string>(), null))
                .Returns(Task.FromResult<PCSCertificateMcPkgsModel>(null));

            var result = await _dut.TryGetCertificateMcPkgsAsync(_plant, new Guid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TryGetMcPkgsByCommPkgNoAndProjectName_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.TryGetCertificateMcPkgsAsync(_plant, new Guid());

            // Assert
            var mcPkg = result.McPkgs.First();
            Assert.AreEqual(_mcPkg1.CommPkgNo, mcPkg.CommPkgNo);
            Assert.AreEqual(_mcPkg1.McPkgNo, mcPkg.McPkgNo);
            Assert.IsFalse(result.CertificateIsAccepted);
        }

        [TestMethod]
        public async Task TryGetCertificateCommPkgsAsync_ShouldReturnCorrectNumberOfCommPkgs()
        {
            // Act
            var result = await _dut.TryGetCertificateCommPkgsAsync(_plant, new Guid());

            // Assert
            Assert.AreEqual(3, result.CommPkgs.Count());
        }

        [TestMethod]
        public async Task TryGetCertificateCommPkgsAsync_ShouldReturnNull_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<PCSCertificateCommPkgsModel>(It.IsAny<string>(), null))
                .Returns(Task.FromResult<PCSCertificateCommPkgsModel>(null));

            var result = await _dut.TryGetCertificateCommPkgsAsync(_plant, new Guid());

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TryGetCertificateCommPkgsAsync_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.TryGetCertificateCommPkgsAsync(_plant, new Guid());

            // Assert
            var commPkg = result.CommPkgs.First();
            Assert.AreEqual(_commPkg1.CommPkgNo, commPkg.CommPkgNo);
            Assert.IsFalse(result.CertificateIsAccepted);
        }
    }
}
