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
        private ProCoSysMcPkgSearchResult _searchPageWithThreeItems;
        private MainApiMcPkgService _dut;

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

            _searchPageWithThreeItems = new ProCoSysMcPkgSearchResult
            {
                Items = new List<ProCoSysMcPkg>
                        {
                            new ProCoSysMcPkg
                            {
                                Id = 111111111,
                                McPkgNo = "McNo1",
                                Description = "Description1"
                            },
                            new ProCoSysMcPkg
                            {
                                Id = 222222222,
                                McPkgNo = "McNo2",
                                Description = "Description2"
                            },
                            new ProCoSysMcPkg
                            {
                                Id = 333333333,
                                McPkgNo = "McNo3",
                                Description = "Description3"
                            }
                        }
            };

            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysMcPkgSearchResult>(It.IsAny<string>()))
                .Returns(Task.FromResult(_searchPageWithThreeItems));

            _dut = new MainApiMcPkgService(_mainApiClient.Object, _mainApiOptions.Object, _plantCache.Object);
        }

        [TestMethod]
        public async Task SearchMcPkgsByMcPkgNo_ShouldReturnCorrectNumberOfMcPkgs()
        {
            // Act
            var result = await _dut.SearchMcPkgsByMcPkgNoAsync(_plant, 2, "C");

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task SearchMcPkgsByMcPkgNo_ShouldThrowException_WhenPlantIsInvalid()
            => await Assert.ThrowsExceptionAsync<ArgumentException>(async ()
                => await _dut.SearchMcPkgsByMcPkgNoAsync("INVALIDPLANT", 2, "A"));

        [TestMethod]
        public async Task SearchMcPkgsByMcPkgNo_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsync<ProCoSysMcPkgSearchResult>(It.IsAny<string>()))
                .Returns(Task.FromResult<ProCoSysMcPkgSearchResult>(null));

            var result = await _dut.SearchMcPkgsByMcPkgNoAsync(_plant, 1, "A");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task SearchMcPkgsByMcPkgNo_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.SearchMcPkgsByMcPkgNoAsync(_plant, 3, "C");

            // Assert
            var mcPkg = result.First();
            Assert.AreEqual(111111111, mcPkg.Id);
            Assert.AreEqual("McNo1", mcPkg.McPkgNo);
            Assert.AreEqual("Description1", mcPkg.Description);
        }
    }
}
