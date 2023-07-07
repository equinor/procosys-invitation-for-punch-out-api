using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
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
        private Mock<IMainApiClient> _foreignApiClient;
        private ProCoSysCommPkgSearchResult _searchPageWithThreeItems;
        private MainApiCommPkgService _dut;

        private const string _plant = "PCS$TESTPLANT";
        private const int _defaultPageSize = 10;
        private const int _defaultCurrentPage = 0;

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<IMainApiClient>();

            _searchPageWithThreeItems = new ProCoSysCommPkgSearchResult
            {
                MaxAvailable = 3,
                Items = new List<ProCoSysSearchCommPkg>
                        {
                            new ProCoSysSearchCommPkg
                            {
                                Id = 111111111,
                                CommPkgNo = "CommNo1",
                                Description = "Description1",
                                CommStatus = "OK",
                                System = "1|2",
                                OperationHandoverStatus = "Accepted"
                            },
                            new ProCoSysSearchCommPkg
                            {
                                Id = 222222222,
                                CommPkgNo = "CommNo2",
                                Description = "Description2",
                                CommStatus = "PA",
                                System = "1|2",
                                OperationHandoverStatus = "Sent"
                            },
                            new ProCoSysSearchCommPkg
                            {
                                Id = 333333333,
                                CommPkgNo = "CommNo3",
                                Description = "Description3",
                                CommStatus = "PB",
                                System = "1|2",
                                OperationHandoverStatus = "Sent"
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
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync(
                _plant,
                "ProjectName",
                "C",
                _defaultPageSize,
                _defaultCurrentPage);

            // Assert
            Assert.AreEqual(3, result.Items.Count);
            Assert.AreEqual(3, result.MaxAvailable);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnEmptyList_WhenSearchingEmptyPage()
        {
            var emptyPage = new ProCoSysCommPkgSearchResult
            {
                MaxAvailable = 3,
                Items = new List<ProCoSysSearchCommPkg>()
            };
            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(emptyPage));
            // Act
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync(
                _plant,
                "ProjectName",
                "C",
                _defaultPageSize,
                1);

            // Assert
            Assert.AreEqual(0, result.Items.Count);
            Assert.AreEqual(3, result.MaxAvailable);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnCommPkgs_WhenSpecifyingNumberOfItems()
        {
            var searchWithOneItem = new ProCoSysCommPkgSearchResult
            {
                MaxAvailable = 3,
                Items = new List<ProCoSysSearchCommPkg>
                {
                    new ProCoSysSearchCommPkg
                    {
                        Id = 111111111,
                        CommPkgNo = "CommNo1",
                        Description = "Description1",
                        CommStatus = "OK"
                    }
                }
            };
            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysCommPkgSearchResult>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(searchWithOneItem));

            // Act
            var result = await _dut.SearchCommPkgsByCommPkgNoAsync(
                _plant,
                "ProjectName",
                "C",
                1,
                _defaultCurrentPage);

            // Assert
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual(3, result.MaxAvailable);
        }

        [TestMethod]
        public async Task SearchCommPkgsByCommPkgNo_ShouldReturnCorrectProperties()
        {
            // Act
            var result =
                await _dut.SearchCommPkgsByCommPkgNoAsync(
                    _plant,
                    "ProjectName",
                    "C",
                    _defaultPageSize,
                    _defaultCurrentPage);

            // Assert
            var commPkg = result.Items.First();
            Assert.AreEqual(111111111, commPkg.Id);
            Assert.AreEqual("CommNo1", commPkg.CommPkgNo);
            Assert.AreEqual("Description1", commPkg.Description);
            Assert.AreEqual("OK", commPkg.CommStatus);
            Assert.AreEqual("1|2", commPkg.System);
            Assert.AreEqual("Accepted", commPkg.OperationHandoverStatus);
        }
    }
}
