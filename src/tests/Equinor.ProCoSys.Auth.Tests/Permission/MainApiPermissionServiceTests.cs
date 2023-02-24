﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
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
        private Mock<IMainApiTokenProvider> _mainApiTokenProvider;

        [TestInitialize]
        public void Setup()
        {
            _mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            _mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            _mainApiClient = new Mock<IMainApiClient>();

            _mainApiTokenProvider = new Mock<IMainApiTokenProvider>();
            _dut = new MainApiPermissionService(_mainApiTokenProvider.Object, _mainApiClient.Object, _mainApiOptions.Object);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldReturnCorrectNumberOfPlants()
        {
            // Arange
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysPlant>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPlant>
                {
                    new ProCoSysPlant { Id = "PCS$ASGARD", Title = "Åsgard" },
                    new ProCoSysPlant { Id = "PCS$ASGARD_A", Title = "ÅsgardA" },
                    new ProCoSysPlant { Id = "PCS$ASGARD_B", Title = "ÅsgardB" },
                }));

            // Act
            var result = await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldSetsCorrectProperties()
        {
            // Arange
            var proCoSysPlant = new ProCoSysPlant { Id = "PCS$ASGARD_B", Title = "ÅsgardB" };
            _mainApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysPlant>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPlant>
                {
                    proCoSysPlant,
                }));
            // Act
            var result = await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            var plant = result.Single();
            Assert.AreEqual(proCoSysPlant.Id, plant.Id);
            Assert.AreEqual(proCoSysPlant.Title, plant.Title);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldSetApplicationAuthentication()
        {
            // Act
            await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            _mainApiTokenProvider.VerifySet(a => a.AuthenticationType = AuthenticationType.AsApplication);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldResetToOnBehalfOfAuthentication()
        {
            // Act
            await _dut.GetAllPlantsForUserAsync(_azureOid);

            // Assert
            _mainApiTokenProvider.VerifySet(a => a.AuthenticationType = AuthenticationType.OnBehalfOf);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnThreePermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<string>{ "A", "B", "C" }));
            // Act
            var result = await _dut.GetPermissionsAsync(_plant);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnNoPermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null) )
                .Returns(Task.FromResult(new List<string>()));
            // Act
            var result = await _dut.GetPermissionsAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPermissions_ShouldReturnNoPermissions_OnInvalidPlant()
        {
            // Act
            var result = await _dut.GetPermissionsAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
 
        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnTwoProjects_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysProject>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysProject>{ new ProCoSysProject(), new ProCoSysProject() }));
            // Act
            var result = await _dut.GetAllOpenProjectsAsync(_plant);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnNoProjects_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<ProCoSysProject>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysProject>()));
            // Act
            var result = await _dut.GetAllOpenProjectsAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetAllOpenProjectsAsync_ShouldReturnNoProjects_OnInvalidPlant()
        {
            // Act
            var result = await _dut.GetAllOpenProjectsAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetContentRestrictionsAsync_ShouldReturnThreePermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<string> { "A", "B", "C" }));
            // Act
            var result = await _dut.GetContentRestrictionsAsync(_plant);

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetContentRestrictionsAsync_ShouldReturnNoPermissions_OnValidPlant()
        {
            // Arrange
            _mainApiClient
                .SetupSequence(x => x.QueryAndDeserializeAsync<List<string>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<string>()));
            // Act
            var result = await _dut.GetContentRestrictionsAsync(_plant);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetContentRestrictionsAsync_ShouldReturnNoPermissions_OnInValidPlant()
        {
            // Act
            var result = await _dut.GetContentRestrictionsAsync("INVALIDPLANT");

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}
