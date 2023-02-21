using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests
{
    // todo move to Equinor.ProCoSys.Auth.Tests project
    [TestClass]
    public class MainApiClientTests
    {
        private Mock<IMainApiTokenProvider> _bearerTokenProvider;
        private Mock<ILogger<MainApiClient>> _logger;

        [TestInitialize]
        public void Setup()
        {
            _bearerTokenProvider = new Mock<IMainApiTokenProvider>();
            _logger = new Mock<ILogger<MainApiClient>>();
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ShouldReturnDeserialized_Object_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "{\"Id\": 123}");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProvider.Object, _logger.Object);

            var response = await dut.QueryAndDeserializeAsync<DummyClass>("url");

            Assert.IsNotNull(response);
            Assert.AreEqual(123, response.Id);
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenRequestIsNotSuccessful_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.BadGateway, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProvider.Object, _logger.Object);

            await Assert.ThrowsExceptionAsync<Exception>(async () => await dut.QueryAndDeserializeAsync<DummyClass>("url"));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenInvalidResponseIsReceived_TestAsync()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProvider.Object, _logger.Object);

            await Assert.ThrowsExceptionAsync<JsonException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>("url"));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenNoUrl()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProvider.Object, _logger.Object);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>(null));
        }

        [TestMethod]
        public async Task QueryAndDeserialize_ThrowsException_WhenUrlTooLong()
        {
            var httpClientFactory = HttpHelper.GetHttpClientFactory(HttpStatusCode.OK, "");
            var dut = new MainApiClient(httpClientFactory, _bearerTokenProvider.Object, _logger.Object);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await dut.QueryAndDeserializeAsync<DummyClass>(new string('u', 2001)));
        }

        private class DummyClass
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Id { get; set; }
        }
    }
}
