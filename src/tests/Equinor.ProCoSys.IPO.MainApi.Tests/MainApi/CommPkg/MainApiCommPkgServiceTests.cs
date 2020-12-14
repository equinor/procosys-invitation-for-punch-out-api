﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.MainApi.CommPkg
{
    [TestClass]
    public class MainApiCommPkgServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IBearerTokenApiClient> _foreignApiClient;
        private ProCoSysCommPkgSearchResult _searchPageWithThreeItems;
        private MainApiCommPkgService _dut;

        private const string _plant = "PCS$TESTPLANT";

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<IBearerTokenApiClient>();

            _searchPageWithThreeItems = new ProCoSysCommPkgSearchResult
            {
                Items = new List<ProCoSysCommPkg>
                        {
                            new ProCoSysCommPkg
                            {
                                Id = 111111111,
                                CommPkgNo = "CommNo1",
                                Description = "Description1",
                                CommStatus = "OK"
                            },
                            new ProCoSysCommPkg
                            {
                                Id = 222222222,
                                CommPkgNo = "CommNo2",
                                Description = "Description2",
                                CommStatus = "PA"
                            },
                            new ProCoSysCommPkg
                            {
                                Id = 333333333,
                                CommPkgNo = "CommNo3",
                                Description = "Description3",
                                CommStatus = "PB"
                            }
                        }
            };

            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(_searchPageWithThreeItems));

            _dut = new MainApiCommPkgService(_foreignApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnCorrectNumberOfCommPkgs()
        {
            // Act
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync(_plant, "ProjectName", "C");

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(It.IsAny<string>(), null))
                .Returns(Task.FromResult<ProCoSysCommPkgSearchResult>(null));

            var result = await _dut.SearchCommPkgsByCommPkgNoAsync(_plant, "ProjectName", "A");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync(_plant, "ProjectName", "C");

            // Assert
            var commPkg = result.First();
            Assert.AreEqual(111111111, commPkg.Id);
            Assert.AreEqual("CommNo1", commPkg.CommPkgNo);
            Assert.AreEqual("Description1", commPkg.Description);
            Assert.AreEqual("OK", commPkg.CommStatus);
        }
    }
}
