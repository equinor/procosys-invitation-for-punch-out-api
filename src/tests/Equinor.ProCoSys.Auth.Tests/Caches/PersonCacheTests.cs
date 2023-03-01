using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Auth.Time;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Caches
{
    [TestClass]
    public class PersonCacheTests
    {
        private PersonCache _dut;
        private ProCoSysPerson _person;
        private readonly Guid _currentUserOid = new("{3BFB54C7-91E2-422E-833F-951AD07FE37F}");
        private Mock<IPersonApiService> _personApiServiceMock;

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            _personApiServiceMock = new Mock<IPersonApiService>();
            _person = new ProCoSysPerson { FirstName = "Erling", LastName = "Braut Haaland"};
            _personApiServiceMock.Setup(p => p.TryGetPersonByOidAsync(_currentUserOid)).ReturnsAsync(_person);

            var optionsMock = new Mock<IOptionsMonitor<CacheOptions>>();
            optionsMock
                .Setup(x => x.CurrentValue)
                .Returns(new CacheOptions());

            _dut = new PersonCache(
                new CacheManager(),
                _personApiServiceMock.Object,
                optionsMock.Object);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnPersonFromPersonApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetAsync(_currentUserOid);

            // Assert
            AssertPerson(result);
            _personApiServiceMock.Verify(p => p.TryGetPersonByOidAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task GetAsync_ShouldReturnPersonsFromCacheSecondTime()
        {
            await _dut.GetAsync(_currentUserOid);

            // Act
            var result = await _dut.GetAsync(_currentUserOid);

            // Assert
            AssertPerson(result);
            // since GetAsync has been called twice, but TryGetPersonByOidAsync has been called once, the second Get uses cache
            _personApiServiceMock.Verify(p => p.TryGetPersonByOidAsync(_currentUserOid), Times.Once);
        }

        private void AssertPerson(ProCoSysPerson person)
        {
            Assert.AreEqual(_person.FirstName, person.FirstName);
            Assert.AreEqual(_person.LastName, person.LastName);
        }
    }
}
