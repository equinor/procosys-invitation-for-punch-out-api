using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.MainApi.Person
{
    [TestClass]
    public class MainApiPersonServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IMainApiClient> _foreignApiClient;
        private MainApiPersonService _dut;
        private ProCoSysPerson _proCoSysPerson1;
        private ProCoSysPerson _proCoSysPerson2;
        private ProCoSysPerson _proCoSysPerson3;

        private const string _plant = "PCS$TESTPLANT";
        private const string _searchString = "A";
        private const string _objectName = "IPO";
        private List<string> _privileges = new List<string>{"SIGN"};

        private List<string> Oids = new List<string>{ "12345678-1234-123456789123", "12345678-1235-123456789123", "12345678-1236-123456789123" };

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });

            _foreignApiClient = new Mock<IMainApiClient>();

            _proCoSysPerson1 = new ProCoSysPerson {
                AzureOid = "12345678-1234-123456789123",
                FirstName = "F1",
                LastName = "L1",
                UserName = "U1",
                Email = "E1"
            };
            _proCoSysPerson2 = new ProCoSysPerson {
                AzureOid = "12345678-1235-123456789123",
                FirstName = "F2",
                LastName = "L2",
                UserName = "U2",
                Email = "E2"
            };
            _proCoSysPerson3 = new ProCoSysPerson {
                AzureOid = "12345678-1236-123456789123",
                FirstName = "F3",
                LastName = "L3",
                UserName = "U3",
                Email = "E3"
            };

            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysPerson>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPerson> { _proCoSysPerson1, _proCoSysPerson2, _proCoSysPerson3 }));

            _dut = new MainApiPersonService(_foreignApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task GetPersons_ShouldReturnCorrectNumberOfPersons()
        {
            // Act
            var result = await _dut.GetPersonsAsync(_plant, _searchString);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetPersons_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysPerson>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPerson>()));

            var result = await _dut.GetPersonsAsync(_plant, _searchString);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPersons_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetPersonsAsync(_plant, _searchString);

            // Assert
            var person = result.First();
            Assert.AreEqual("12345678-1234-123456789123", person.AzureOid);
            Assert.AreEqual("F1", person.FirstName);
            Assert.AreEqual("L1", person.LastName);
            Assert.AreEqual("U1", person.UserName);
            Assert.AreEqual("E1", person.Email);
        }

        [TestMethod]
        public async Task GetPersonsWithPrivileges_ShouldReturnCorrectNumberOfPersons()
        {
            // Act
            var result = await _dut.GetPersonsWithPrivilegesAsync(_plant, _searchString, _objectName, _privileges);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetPersonsWithPrivileges_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysPerson>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPerson>()));

            var result = await _dut.GetPersonsWithPrivilegesAsync(_plant, _searchString, _objectName, _privileges);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPersonsWithPrivileges_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetPersonsWithPrivilegesAsync(_plant, _searchString, _objectName, _privileges);

            // Assert
            var person = result.First();
            Assert.AreEqual("12345678-1234-123456789123", person.AzureOid);
            Assert.AreEqual("F1", person.FirstName);
            Assert.AreEqual("L1", person.LastName);
            Assert.AreEqual("U1", person.UserName);
            Assert.AreEqual("E1", person.Email);
        }

        [TestMethod]
        public async Task GetPersonsByOidsAsync_ShouldReturnCorrectNumberOfPersons()
        {
            // Act
            var result = await _dut.GetPersonsByOidsAsync(_plant, Oids);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetPersonsByOidsAsync_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysPerson>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPerson>()));

            var result = await _dut.GetPersonsByOidsAsync(_plant, Oids);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPersonsByOidsAsync_ShouldReturnCorrectProperties()
        {
            // Act
            var result = await _dut.GetPersonsByOidsAsync(_plant, Oids);

            // Assert
            var person = result.First();
            Assert.AreEqual("12345678-1234-123456789123", person.AzureOid);
            Assert.AreEqual("F1", person.FirstName);
            Assert.AreEqual("L1", person.LastName);
            Assert.AreEqual("U1", person.UserName);
            Assert.AreEqual("E1", person.Email);
        }

        [TestMethod]
        public async Task GetPersonByOidWithPrivilegesAsync_ShouldReturnCorrectNumberOfPersons()
        {
            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysPerson>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(_proCoSysPerson1));
            // Act
            var result = await _dut.GetPersonByOidWithPrivilegesAsync(_plant, Oids[0], _objectName, _privileges);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetPersonByOidWithPrivilegesAsync_ShouldReturnEmptyList_WhenResultIsInvalid()
        {
            _foreignApiClient
                .Setup(x => x.QueryAndDeserializeAsync<ProCoSysPerson>(It.IsAny<string>(), null))
                .Returns(Task.FromResult<ProCoSysPerson>(null));

            var result = await _dut.GetPersonByOidWithPrivilegesAsync(_plant, Oids[0], _objectName, _privileges);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetPersonByOidWithPrivilegesAsync_ShouldReturnCorrectProperties()
        {
            _foreignApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<ProCoSysPerson>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(_proCoSysPerson1));
            // Act
            var person = await _dut.GetPersonByOidWithPrivilegesAsync(_plant, Oids[0], _objectName, _privileges);

            // Assert
            Assert.AreEqual("12345678-1234-123456789123", person.AzureOid);
            Assert.AreEqual("F1", person.FirstName);
            Assert.AreEqual("L1", person.LastName);
            Assert.AreEqual("U1", person.UserName);
            Assert.AreEqual("E1", person.Email);
        }
    }
}
