using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Permission;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Permission
{
    [TestClass]
    public class MainApiPermissionServiceTests
    {
        private readonly Guid _azureOid = Guid.NewGuid();
        private const string _plant = "PCS$TESTPLANT";
        private Mock<IOptionsMonitor<MainApiOptions>> _mainApiOptions;
        private Mock<IMainApiClient> _mainApiClient;
        private MainApiPermissionService _dut;

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            _mainApiClient = new Mock<IMainApiClient>();

            _dut = new MainApiPermissionService(_mainApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldReturnCorrectNumberOfPlants()
        {
            // Arange
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsApplicationAsync<List<AccessablePlant>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<AccessablePlant>
                {
                    new() { Id = "PCS$ASGARD", Title = "Åsgard" },
                    new() { Id = "PCS$ASGARD_A", Title = "ÅsgardA" },
                    new() { Id = "PCS$ASGARD_B", Title = "ÅsgardB" },
                });

            // Act
            var result = await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldSetsCorrectProperties()
        {
            // Arange
            var proCoSysPlant = new AccessablePlant { Id = "PCS$ASGARD_B", Title = "ÅsgardB" };
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsApplicationAsync<List<AccessablePlant>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<AccessablePlant> { proCoSysPlant });
            // Act
            var result = await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            var plant = result.Single();
            Assert.AreEqual(proCoSysPlant.Id, plant.Id);
            Assert.AreEqual(proCoSysPlant.Title, plant.Title);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnThreePermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<string>{ "A", "B", "C" });
            // Act
            var result = await _dut.GetPermissionsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnNoPermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null) )
                .ReturnsAsync(new List<string>());
            // Act
            var result = await _dut.GetPermissionsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnNoPermissions_OnInvalidPlant()
        {
            // Act
            var result = await _dut.GetPermissionsForCurrentUserAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
 
        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnTwoProjects_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<AccessableProject>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<AccessableProject>{ new AccessableProject(), new AccessableProject() });
            // Act
            var result = await _dut.GetAllOpenProjectsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnNoProjects_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<AccessableProject>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<AccessableProject>());
            // Act
            var result = await _dut.GetAllOpenProjectsForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnNoProjects_OnInvalidPlant()
        {
            // Act
            var result = await _dut.GetAllOpenProjectsForCurrentUserAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetRestrictionRolesAsync_ShouldReturnThreePermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<string> { "A", "B", "C" });
            // Act
            var result = await _dut.GetRestrictionRolesForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetRestrictionRolesAsync_ShouldReturnNoPermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null))
                .ReturnsAsync(new List<string>());
            // Act
            var result = await _dut.GetRestrictionRolesForCurrentUserAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetRestrictionRolesAsync_ShouldReturnNoPermissions_OnInValidPlant()
        {
            // Act
            var result = await _dut.GetRestrictionRolesForCurrentUserAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}
