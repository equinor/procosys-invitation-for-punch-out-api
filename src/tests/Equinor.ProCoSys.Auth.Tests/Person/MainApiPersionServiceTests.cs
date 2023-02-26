using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Person;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Person
{
    [TestClass]
    public class MainApiPersionServiceTests
    {
        private readonly Guid _azureOid = Guid.NewGuid();
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IMainApiClient> _mainApiClient;
        private MainApiPersonService _dut;
        private Mock<IMainApiAuthenticator> _mainApiTokenProvider;

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            _mainApiClient = new Mock<IMainApiClient>();

            _mainApiTokenProvider = new Mock<IMainApiAuthenticator>();
            _dut = new MainApiPersonService(_mainApiTokenProvider.Object, _mainApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task TryGetPersonByOidAsync_ShouldReturnPerson()
        {
            // Arange
            var person = new ProCoSysPerson { FirstName = "Lars", LastName = "Monsen" };
            _mainApiClient
                .Setup(x => x.TryQueryAndDeserializeAsync<ProCoSysPerson>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(person));

            // Act
            var result = await _dut.TryGetPersonByOidAsync(_azureOid);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(person.FirstName, result.FirstName);
            Assert.AreEqual(person.LastName, result.LastName);
        }

        [TestMethod]
        public async Task TryGetPersonByOidAsync_ShouldSetApplicationAuthentication()
        {
            // Act
            await _dut.TryGetPersonByOidAsync(_azureOid);

            // Assert
            _mainApiTokenProvider.VerifySet(a => a.AuthenticationType = AuthenticationType.AsApplication);
        }

        [TestMethod]
        public async Task TryGetPersonByOidAsync_ShouldResetToOnBehalfOfAuthentication()
        {
            // Act
            await _dut.TryGetPersonByOidAsync(_azureOid);

            // Assert
            _mainApiTokenProvider.VerifySet(a => a.AuthenticationType = AuthenticationType.OnBehalfOf);
        }
    }
}
