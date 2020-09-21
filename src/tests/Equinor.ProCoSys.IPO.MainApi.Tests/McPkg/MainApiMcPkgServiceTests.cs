using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Equinor.ProCoSys.IPO.MainApi.McPkg;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.MainApi.Tests.McPkg
{
    [TestClass]
    public class MainApiMcPkgServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IBearerTokenApiClient> _mainApiClient;
        private Mock<IPlantCache> _plantCache;
        private MainApiMcPkgService _dut;
        private ProCoSysMcPkg _proCoSysMcPkg1;
        private ProCoSysMcPkg _proCoSysMcPkg2;
        private ProCoSysMcPkg _proCoSysMcPkg3;

        private const string _plant = "PCS$TESTPLANT";

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });

            _mainApiClient = new Mock<IBearerTokenApiClient>();
            _plantCache = new Mock<IPlantCache>();
            _plantCache
                .Setup(x => x.IsValidPlantForCurrentUserAsync(_plant))
                .Returns(Task.FromResult(true));

            _proCoSysMcPkg1 = new ProCoSysMcPkg {Id = 111111111, McPkgNo = "McNo1", Description = "Description1"};
            _proCoSysMcPkg2 = new ProCoSysMcPkg {Id = 222222222, McPkgNo = "McNo2", Description = "Description2"};
            _proCoSysMcPkg3 = new ProCoSysMcPkg {Id = 333333333, McPkgNo = "McNo3", Description = "Description3"};

            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProCoSysMcPkg> { _proCoSysMcPkg1, _proCoSysMcPkg2, _proCoSysMcPkg3 }));

            _dut = new MainApiMcPkgService(_mainApiClient.Object, _mainApiOptions.Object, _plantCache.Object);
        }

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldReturnCorrectNumberOfMcPkgs()
        {
            // Act
            var result = await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync(_plant, "Project2", "C");

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldThrowException_WhenPlantIsInvalid()
            => await Assert.ThrowsExceptionAsync<ArgumentException>(async ()
                => await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync("INVALIDPLANT", "Project2", "A"));

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysMcPkg>>(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<ProCoSysMcPkg>()));

            var result = await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync(_plant, "Project1", "A");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetMcPkgsByCommPkgNoAndProjectName_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetMcPkgsByCommPkgNoAndProjectNameAsync(_plant, "Project3", "C");

            // Assert
            var mcPkg = result.First();
            Assert.AreEqual(111111111, mcPkg.Id);
            Assert.AreEqual("McNo1", mcPkg.McPkgNo);
            Assert.AreEqual("Description1", mcPkg.Description);
        }
    }
}
