using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.MainApi.Project
{
    [TestClass]
    public class MainApiForApplicationProjectServiceTests
    {
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IMainApiClientForApplication> _mainApiClient;
        private ProCoSysProject _proCoSysProject1;
        private ProCoSysProject _proCoSysProject2;
        private MainApiForApplicationProjectService _dut;

        private const string _plant = "PCS$TESTPLANT";
        private const string _project1Name = "NameA";
        private const string _project1Description = "Description1";

        [TestInitialize]
        public void Setup()
        {
            const string Project2Name = "NameB";
            const string Project2Description = "Description2";

            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            _mainApiClient = new Mock<IMainApiClientForApplication>();

            _proCoSysProject1 = new ProCoSysProject { Id = 1, Name = _project1Name, Description = _project1Description };
            _proCoSysProject2 = new ProCoSysProject { Id = 2, Name = Project2Name, Description = Project2Description };

            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysProject>>(It.IsAny<string>(), It.IsAny<CancellationToken>(), null))
                .Returns(Task.FromResult(new List<ProCoSysProject> { _proCoSysProject1, _proCoSysProject2 }));

            _dut = new MainApiForApplicationProjectService(_mainApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task TryGetProject_ShouldReturnProject()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.TryQueryAndDeserializeAsync<ProCoSysProject>(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<List<KeyValuePair<string, string>>>()))
                .Returns(Task.FromResult(_proCoSysProject1));

            // Act
            var result = await _dut.TryGetProjectAsync(_plant, _project1Name, CancellationToken.None);

            // Assert
            Assert.AreEqual(_project1Name, result.Name);
            Assert.AreEqual(_project1Description, result.Description);
        }
    }
}
