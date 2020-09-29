using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.LibraryApi.FunctionalRole
{
    [TestClass]
    public class MainApiFunctionalRoleServiceTests
    {
        private Mock<IOptionsMonitor<LibraryApiOptions>> _mainApiOptions;
        private Mock<IBearerTokenApiClient> _foreignApiClient;
        private Mock<IPlantCache> _plantCache;
        private LibraryApiFunctionalRoleService _dut;
        private ProCoSysFunctionalRole _proCoSysFunctionalRole1;
        private ProCoSysFunctionalRole _proCoSysFunctionalRole2;

        private const string _plant = "PCS$TESTPLANT";
        private const string _classification = "NOTIFICATION";
        private static List<KeyValuePair<string, string>> _extraHeaders = new List<KeyValuePair<string, string>>();

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<LibraryApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new LibraryApiOptions { BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<IBearerTokenApiClient>();
            _plantCache = new Mock<IPlantCache>();
            _plantCache
                .Setup(x => x.IsValidPlantForCurrentUserAsync(_plant))
                .Returns(Task.FromResult(true));

            _proCoSysFunctionalRole1 = new ProCoSysFunctionalRole
            {
                Code = "A",
                Description = "Description1",
                Email = "example1@email.com",
                InformationEmail = "infoexample1@email.com",
                UsePersonalEmail = true
            };
            _proCoSysFunctionalRole2 = new ProCoSysFunctionalRole
            {
                Code = "B",
                Description = "Description2",
                Email = "example2@email.com",
                InformationEmail = "infoexample2@email.com",
                UsePersonalEmail = false
            };

            _extraHeaders = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("x-plant", _plant) };

            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(It.IsAny<string>(), _extraHeaders))
                .Returns(Task.FromResult(new List<ProCoSysFunctionalRole> { _proCoSysFunctionalRole1, _proCoSysFunctionalRole2 }));

            _dut = new LibraryApiFunctionalRoleService(_foreignApiClient.Object, _mainApiOptions.Object, _plantCache.Object);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldReturnCorrectNumberOfFunctionalRoles()
        {
            // Act
            var result = await _dut.GetFunctionalRolesByClassificationAsync(_plant, _classification);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldThrowException_WhenPlantIsInvalid()
            => await Assert.ThrowsExceptionAsync<ArgumentException>(async ()
                => await _dut.GetFunctionalRolesByClassificationAsync("INVALIDPLANT", _classification));

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(It.IsAny<string>(), _extraHeaders))
                .Returns(Task.FromResult(new List<ProCoSysFunctionalRole>()));

            var result = await _dut.GetFunctionalRolesByClassificationAsync(_plant, "NOTACLASSIFICATION");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetFunctionalRolesByClassificationAsync(_plant, _classification);

            // Assert
            var functionalRole = result.First();
            Assert.AreEqual("A", functionalRole.Code);
            Assert.AreEqual("Description1", functionalRole.Description);
            Assert.AreEqual("example1@email.com", functionalRole.Email);
            Assert.AreEqual("infoexample1@email.com", functionalRole.InformationEmail);
            Assert.IsTrue(functionalRole.UsePersonalEmail != null && functionalRole.UsePersonalEmail.Value);
        }
    }
}
