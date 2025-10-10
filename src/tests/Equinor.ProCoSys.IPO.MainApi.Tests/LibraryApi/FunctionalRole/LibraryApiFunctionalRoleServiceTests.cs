using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.LibraryApi.FunctionalRole
{
    [TestClass]
    public class LibraryApiFunctionalRoleServiceTests
    {
        private Mock<IOptionsMonitor<LibraryApiOptions>> _libraryApiOptions;
        private Mock<ILibraryApiForUserClient> _foreignApiClient;
        private LibraryApiFunctionalRoleService _dut;
        private ProCoSysFunctionalRole _proCoSysFunctionalRole1;
        private ProCoSysFunctionalRole _proCoSysFunctionalRole2;

        private const string _plant = "PCS$TESTPLANT";
        private const string _classification = "NOTIFICATION";
        private List<string> _functionalRoleCodes = new List<string> { "A", "B" };
        private static List<KeyValuePair<string, string>> _extraHeaders = new List<KeyValuePair<string, string>>();

        [TestInitialize]
        public void Setup()
        {
            _libraryApiOptions = new Mock<IOptionsMonitor<LibraryApiOptions>>();
            _libraryApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new LibraryApiOptions { BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<ILibraryApiForUserClient>();

            _proCoSysFunctionalRole1 = new ProCoSysFunctionalRole
            {
                Code = "A",
                Description = "Description1",
                Email = "example1@email.com",
                InformationEmail = "infoexample1@email.com",
                UsePersonalEmail = true,
                Persons = new List<ProCoSysPerson>
                {
                    new ProCoSysPerson{
                        AzureOid = new Guid("11111111-1111-2222-2222-333333333333").ToString(),
                        Email = "ola@test.com",
                        FirstName = "Ola",
                        LastName = "Nordmann",
                        UserName = "ON"
                    }
                }
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
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>(),
                    _extraHeaders))
                .Returns(Task.FromResult(new List<ProCoSysFunctionalRole> { _proCoSysFunctionalRole1, _proCoSysFunctionalRole2 }));

            _dut = new LibraryApiFunctionalRoleService(_foreignApiClient.Object, _libraryApiOptions.Object);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldReturnCorrectNumberOfFunctionalRoles()
        {
            // Act
            var result = await _dut.GetFunctionalRolesByClassificationAsync(
                _plant,
                _classification,
                CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(It.IsAny<string>(), It.IsAny<CancellationToken>(), _extraHeaders))
                .Returns(Task.FromResult(new List<ProCoSysFunctionalRole>()));

            var result = await _dut.GetFunctionalRolesByClassificationAsync(
                _plant,
                "NOTACLASSIFICATION",
                CancellationToken.None);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByClassification_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetFunctionalRolesByClassificationAsync(
                _plant,
                _classification,
                CancellationToken.None);

            // Assert
            var functionalRole = result.First();
            Assert.AreEqual("A", functionalRole.Code);
            Assert.AreEqual("Description1", functionalRole.Description);
            Assert.AreEqual("example1@email.com", functionalRole.Email);
            Assert.AreEqual("infoexample1@email.com", functionalRole.InformationEmail);
            Assert.IsTrue(functionalRole.UsePersonalEmail != null && functionalRole.UsePersonalEmail.Value);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByCode_ShouldReturnCorrectNumberOfFunctionalRoles()
        {
            // Act
            var result = await _dut.GetFunctionalRolesByCodeAsync(
                _plant,
                _functionalRoleCodes,
                CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByCode_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>(),
                    _extraHeaders))
                .Returns(Task.FromResult(new List<ProCoSysFunctionalRole>()));

            var result = await _dut.GetFunctionalRolesByCodeAsync(
                _plant,
                new List<string> { "not a code" },
                CancellationToken.None);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByCode_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetFunctionalRolesByCodeAsync(
                _plant,
                _functionalRoleCodes,
                CancellationToken.None);

            // Assert
            var functionalRole = result.First();
            Assert.AreEqual("A", functionalRole.Code);
            Assert.AreEqual("Description1", functionalRole.Description);
            Assert.AreEqual("example1@email.com", functionalRole.Email);
            Assert.AreEqual("infoexample1@email.com", functionalRole.InformationEmail);
            Assert.IsTrue(functionalRole.UsePersonalEmail != null && functionalRole.UsePersonalEmail.Value);
            var person = functionalRole.Persons.First();
            Assert.AreEqual("Ola", person.FirstName);
            Assert.AreEqual("Nordmann", person.LastName);
            Assert.AreEqual("ola@test.com", person.Email);
            Assert.AreEqual(new Guid("11111111-1111-2222-2222-333333333333").ToString(), person.AzureOid);
        }

        [TestMethod]
        public async Task GetFunctionalRolesByCode_ShouldReturnEncodedFunctionalRoleCode_WhenFunctionalRoleCodeIncludesAndSign()
        {
            // Arrange
            var url = string.Empty;
            _foreignApiClient.Setup(h => h.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<KeyValuePair<string, string>>>()))
                    .Callback<string, CancellationToken, List<KeyValuePair<string, string>>>((r, _, _) => url = r);

            // Act
            await _dut.GetFunctionalRolesByCodeAsync(_plant, new List<string> { "C&D" }, CancellationToken.None);

            // Assert
            _foreignApiClient.Verify(x => x.QueryAndDeserializeAsync<List<ProCoSysFunctionalRole>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<List<KeyValuePair<string, string>>>()), Times.Once);
            Assert.IsTrue(url.Equals("http://example.com/FunctionalRolesByCodes?classification=IPO&functionalRoleCodes=C%26D"),
                "Expected url encoded functional role code");
        }
    }
}
