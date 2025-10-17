using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.MainApi.McPkg
{
    [TestClass]
    public class MainApiForUserMcPkgServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IMainApiClientForUser> _foreignApiClient;
        private MainApiForUserMcPkgService _dut;
        private ProCoSysMcPkgOnCommPkg _proCoSysMcPkgOnCommPkg1;
        private ProCoSysMcPkgOnCommPkg _proCoSysMcPkgOnCommPkg2;
        private ProCoSysMcPkgOnCommPkg _proCoSysMcPkgOnCommPkg3;
        private ProCoSysMcPkg _proCoSysMcPkg1;
        private ProCoSysMcPkg _proCoSysMcPkg2;
        private ProCoSysMcPkg _proCoSysMcPkg3;

        private const string _plant = "PCS$TESTPLANT";
        private DateTime RfocAcceptedAt = new DateTime(2016, 7, 8);

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<IMainApiClientForUser>();

            _proCoSysMcPkgOnCommPkg1 = new ProCoSysMcPkgOnCommPkg
            {
                Id = 111111111,
                McPkgNo = "McNo1",
                Description = "Description1",
                DisciplineCode = "A",
                System = "1|2",
                OperationHandoverStatus = "Accepted",
                M01 = new DateTime(2021, 10, 10),
                M02 = null,
                Status = "OK",
                RfocAcceptedAt = RfocAcceptedAt
            };
            _proCoSysMcPkgOnCommPkg2 = new ProCoSysMcPkgOnCommPkg
            {
                Id = 222222222,
                McPkgNo = "McNo2",
                Description = "Description2",
                DisciplineCode = "A",
                System = "1|2",
                OperationHandoverStatus = "Accepted",
                M01 = new DateTime(),
                M02 = null,
                Status = "PA",
                RfocAcceptedAt = RfocAcceptedAt
            };
            _proCoSysMcPkgOnCommPkg3 = new ProCoSysMcPkgOnCommPkg
            {
                Id = 333333333,
                McPkgNo = "McNo3",
                Description = "Description3",
                DisciplineCode = "B",
                System = "1|2",
                OperationHandoverStatus = "Accepted",
                M01 = new DateTime(),
                M02 = null,
                Status = "PB",
                RfocAcceptedAt = RfocAcceptedAt
            };

            _proCoSysMcPkg1 = new ProCoSysMcPkg
            {
                Id = 111111111,
                McPkgNo = "McNo1",
                Description = "Description1",
                DisciplineCode = "A",
                System = "1|2",
                OperationHandoverStatus = "Accepted",
                M01 = new DateTime(2021, 10, 10),
                M02 = null,
                RfocAcceptedAt = RfocAcceptedAt
            };
            _proCoSysMcPkg2 = new ProCoSysMcPkg
            {
                Id = 222222222,
                McPkgNo = "McNo2",
                Description = "Description2",
                DisciplineCode = "A",
                System = "1|2",
                OperationHandoverStatus = "Accepted",
                M01 = new DateTime(),
                M02 = null,
                RfocAcceptedAt = RfocAcceptedAt
            };
            _proCoSysMcPkg3 = new ProCoSysMcPkg
            {
                Id = 333333333,
                McPkgNo = "McNo3",
                Description = "Description3",
                DisciplineCode = "B",
                System = "1|2",
                OperationHandoverStatus = "Accepted",
                M01 = new DateTime(),
                M02 = null,
                RfocAcceptedAt = RfocAcceptedAt
            };

            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysMcPkgOnCommPkg>>(It.IsAny<string>(), It.IsAny<CancellationToken>(), null))
                .Returns(Task.FromResult(new List<ProCoSysMcPkgOnCommPkg> { _proCoSysMcPkgOnCommPkg1, _proCoSysMcPkgOnCommPkg2, _proCoSysMcPkgOnCommPkg3 }));

            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(It.IsAny<string>(), It.IsAny<CancellationToken>(), null))
                .Returns(Task.FromResult(new List<ProCoSysMcPkg> { _proCoSysMcPkg1, _proCoSysMcPkg2, _proCoSysMcPkg3 }));

            _foreignApiClient
                .Setup(x => x.PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _dut = new MainApiForUserMcPkgService(_foreignApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldReturnCorrectNumberOfMcPkgs()
        {
            // Act
            var result = await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync(_plant, "Project2", "C", CancellationToken.None);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysMcPkgOnCommPkg>>(It.IsAny<string>(), It.IsAny<CancellationToken>(), null))
                .Returns(Task.FromResult(new List<ProCoSysMcPkgOnCommPkg>()));

            var result = await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync(_plant, "Project1", "A", CancellationToken.None);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync(_plant, "Project3", "C", CancellationToken.None);

            // Assert
            var mcPkg = result.First();
            Assert.AreEqual(111111111, mcPkg.Id);
            Assert.AreEqual("McNo1", mcPkg.McPkgNo);
            Assert.AreEqual("Description1", mcPkg.Description);
            Assert.AreEqual("A", mcPkg.DisciplineCode);
            Assert.AreEqual("1|2", mcPkg.System);
            Assert.AreEqual("Accepted", mcPkg.OperationHandoverStatus);
            Assert.AreEqual(RfocAcceptedAt, mcPkg.RfocAcceptedAt);
            Assert.AreEqual(new DateTime(2021, 10, 10), mcPkg.M01);
            Assert.IsNull(mcPkg.M02);
        }

        [TestMethod]
        public async Task GetMcPkgsByMcPkgNosAsync_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetMcPkgsByMcPkgNosAsync(_plant, "Project3", new List<string> { "McNo1", "McNo2", "McNo3" }, CancellationToken.None);

            // Assert
            Assert.AreEqual(3, result.Count);
            var mcPkg = result.First();
            Assert.AreEqual(111111111, mcPkg.Id);
            Assert.AreEqual("McNo1", mcPkg.McPkgNo);
            Assert.AreEqual("Description1", mcPkg.Description);
            Assert.AreEqual("A", mcPkg.DisciplineCode);
            Assert.AreEqual("1|2", mcPkg.System);
            mcPkg = result[1];
            Assert.AreEqual(222222222, mcPkg.Id);
            Assert.AreEqual("McNo2", mcPkg.McPkgNo);
            Assert.AreEqual("Description2", mcPkg.Description);
            Assert.AreEqual("A", mcPkg.DisciplineCode);
            Assert.AreEqual(RfocAcceptedAt, mcPkg.RfocAcceptedAt);
        }

        [TestMethod]
        public async Task GetMcPkgsByMcPkgNosAsync_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(It.IsAny<string>(), It.IsAny<CancellationToken>(), null))
                .Returns(Task.FromResult(new List<ProCoSysMcPkg>()));

            var result =
                await _dut.GetMcPkgsByMcPkgNosAsync(_plant, "Project3", new List<string> { "McNo1", "McNo2", "McNo3" }, CancellationToken.None);

            Assert.AreEqual(0, result.Count);
        }
    }
}
