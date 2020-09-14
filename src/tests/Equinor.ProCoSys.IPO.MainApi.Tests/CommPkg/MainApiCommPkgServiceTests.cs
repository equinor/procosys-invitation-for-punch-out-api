using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MainApi.Client;
using Equinor.ProCoSys.IPO.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.MainApi.Tests.CommPkg
{
    [TestClass]
    public class MainApiCommPkgServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IBearerTokenApiClient> _mainApiClient;
        private Mock<IPlantCache> _plantCache;
        private ProCoSysCommPkgSearchResult _searchPageWithThreeItems;
        private MainApiCommPkgService _dut;

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
                .Setup(x => x.IsValidPlantForCurrentUserAsync("PCS$TESTPLANT"))
                .Returns(Task.FromResult(true));

            _searchPageWithThreeItems = new ProCoSysCommPkgSearchResult
            {
                Items = new List<ProCoSysCommPkg>
                        {
                            new ProCoSysCommPkg
                            {
                                Description = "Description1",
                                Id = 111111111,
                                CommPkgNo = "CommNo1",
                                CommStatus = "OK"
                            },
                            new ProCoSysCommPkg
                            {
                                Description = "Description2",
                                Id = 222222222,
                                CommPkgNo = "CommNo2",
                                CommStatus = "PA"
                            },
                            new ProCoSysCommPkg
                            {
                                Description = "Description3",
                                Id = 333333333,
                                CommPkgNo = "CommNo3",
                                CommStatus = "PB"
                            }
                        }
            };

            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(It.IsAny<string>()))
                .Returns(Task.FromResult(_searchPageWithThreeItems));

            _dut = new MainApiCommPkgService(_mainApiClient.Object, _mainApiOptions.Object, _plantCache.Object);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnCorrectNumberOfCommPkgs()
        {
            // Act
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync("PCS$TESTPLANT", 2, "C");

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldThrowException_WhenPlantIsInvalid()
            => await Assert.ThrowsExceptionAsync<ArgumentException>(async ()
                => await _dut.SearchCommPkgsByCommPkgNoAsync("INVALIDPLANT", 2, "A"));

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(It.IsAny<string>()))
                .Returns(Task.FromResult<ProCoSysCommPkgSearchResult>(null));

            var result = await _dut.SearchCommPkgsByCommPkgNoAsync("PCS$TESTPLANT", 1, "A");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync("PCS$TESTPLANT", 3, "C");

            // Assert
            var commPkg = result.First();
            Assert.AreEqual("Description1", commPkg.Description);
            Assert.AreEqual(111111111, commPkg.Id);
            Assert.AreEqual("CommNo1", commPkg.CommPkgNo);
            Assert.AreEqual("OK", commPkg.CommStatus);
        }
    }
}
