using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Time;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Caches
{
    [TestClass]
    public class PermissionCacheTests
    {
        private PermissionCache _dut;
        private readonly Guid _currentUserOid = new Guid("{3BFB54C7-91E2-422E-833F-951AD07FE37F}");
        private Mock<IPermissionApiService> _permissionApiServiceMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private readonly string Plant1IdWithAccess = "P1";
        private readonly string Plant2IdWithAccess = "P2";
        private readonly string PlantIdWithoutAccess = "P3";
        private readonly string Plant1TitleWithAccess = "P1 Title";
        private readonly string Plant2TitleWithAccess = "P2 Title";
        private readonly string PlantTitleWithoutAccess = "P3 Title";
        private readonly string Permission1 = "A";
        private readonly string Permission2 = "B";
        private readonly string Project1WithAccess = "P1";
        private readonly string Project2WithAccess = "P2";
        private readonly string ProjectWithoutAccess = "P3";
        private readonly string Restriction1 = "R1";
        private readonly string Restriction2 = "R2";

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            _permissionApiServiceMock = new Mock<IPermissionApiService>();
            _permissionApiServiceMock.Setup(p => p.GetAllPlantsForUserAsync(_currentUserOid)).Returns(Task.FromResult(
                new List<AccessablePlant>
                {
                    new AccessablePlant
                    {
                        Id = Plant1IdWithAccess,
                        Title = Plant1TitleWithAccess,
                        HasAccess = true
                    },
                    new AccessablePlant
                    {
                        Id = Plant2IdWithAccess,
                        Title = Plant2TitleWithAccess,
                        HasAccess = true
                    },
                    new AccessablePlant
                    {
                        Id = PlantIdWithoutAccess,
                        Title = PlantTitleWithoutAccess
                    }
                }));
            _permissionApiServiceMock.Setup(p => p.GetAllOpenProjectsForCurrentUserAsync(Plant1IdWithAccess))
                .Returns(Task.FromResult(new List<AccessableProject>
                {
                    new AccessableProject {Name = Project1WithAccess, HasAccess = true},
                    new AccessableProject {Name = Project2WithAccess, HasAccess = true},
                    new AccessableProject {Name = ProjectWithoutAccess}
                }));
            _permissionApiServiceMock.Setup(p => p.GetPermissionsForCurrentUserAsync(Plant1IdWithAccess))
                .Returns(Task.FromResult(new List<string> {Permission1, Permission2}));
            _permissionApiServiceMock.Setup(p => p.GetContentRestrictionsForCurrentUserAsync(Plant1IdWithAccess))
                .Returns(Task.FromResult(new List<string> { Restriction1, Restriction2 }));

            var optionsMock = new Mock<IOptionsMonitor<CacheOptions>>();
            optionsMock
                .Setup(x => x.CurrentValue)
                .Returns(new CacheOptions());

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock.Setup(c => c.GetCurrentUserOid()).Returns(_currentUserOid);

            _dut = new PermissionCache(
                new CacheManager(),
                _currentUserProviderMock.Object,
                _permissionApiServiceMock.Object,
                optionsMock.Object);
        }

        [TestMethod]
        public async Task GetPlantIdsWithAccessForUserAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetPlantIdsWithAccessForUserAsync(_currentUserOid);

            // Assert
            AssertPlants(result);
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task GetPlantIdsWithAccessForUserAsync_ShouldReturnPlantsFromCacheSecondTime()
        {
            await _dut.GetPlantIdsWithAccessForUserAsync(_currentUserOid);

            // Act
            var result = await _dut.GetPlantIdsWithAccessForUserAsync(_currentUserOid);

            // Assert
            AssertPlants(result);
            // since GetPlantIdsWithAccessForUserAsyncAsync has been called twice, but GetAllPlantsAsync has been called once, the second Get uses cache
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlant_ShouldReturnTrue_WhenKnownPlant()
        {
            // Act
            var result = await _dut.HasCurrentUserAccessToPlantAsync(Plant2IdWithAccess);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlant_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.HasCurrentUserAccessToPlantAsync("XYZ");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlant_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            await _dut.HasCurrentUserAccessToPlantAsync(Plant2IdWithAccess);

            // Assert
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlant_ShouldReturnPlantsFromCacheSecondTime()
        {
            await _dut.HasCurrentUserAccessToPlantAsync("XYZ");
            // Act
            await _dut.HasCurrentUserAccessToPlantAsync(Plant2IdWithAccess);

            // Assert
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task HasUserAccessToPlant_ShouldReturnTrue_WhenKnownPlant()
        {
            // Act
            var result = await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasUserAccessToPlant_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.HasUserAccessToPlantAsync("XYZ", _currentUserOid);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasUserAccessToPlant_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid);

            // Assert
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task HasUserAccessToPlant_ShouldReturnPlantsFromCache()
        {
            await _dut.HasUserAccessToPlantAsync("ABC", _currentUserOid);
            // Act
            await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid);

            // Assert
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);
        }

        [TestMethod]
        public async Task IsAValidPlant_ShouldReturnTrue_WhenKnownPlantWithAccess()
        {
            // Act
            var result = await _dut.IsAValidPlantAsync(Plant2IdWithAccess);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsAValidPlant_ShouldReturnTrue_WhenKnownPlantWithoutAccess()
        {
            // Act
            var result = await _dut.IsAValidPlantAsync(PlantIdWithoutAccess);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsAValidPlant_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.IsAValidPlantAsync("XYZ");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetPlantTitleAsync_ShouldReturnPlant_WhenKnownPlantWithAccess()
        {
            // Act
            var result = await _dut.GetPlantTitleAsync(Plant2IdWithAccess);

            // Assert
            Assert.AreEqual(Plant2TitleWithAccess, result);
        }

        [TestMethod]
        public async Task GetPlantTitleAsync_ShouldReturnPlant_WhenKnownPlantWithoutAccess()
        {
            // Act
            var result = await _dut.GetPlantTitleAsync(PlantIdWithoutAccess);

            // Assert
            Assert.AreEqual(PlantTitleWithoutAccess, result);
        }

        [TestMethod]
        public async Task GetPlantTitleAsync_ShouldReturnNull_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.GetPlantTitleAsync("XYZ");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Clear_ShouldForceGettingPlantsFromApiServiceAgain()
        {
            // Arrange
            var result = await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid);
            Assert.IsTrue(result);
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Once);

            // Act
            _dut.ClearAll(Plant2IdWithAccess, _currentUserOid);

            // Assert
            result = await _dut.HasUserAccessToPlantAsync(Plant2IdWithAccess, _currentUserOid);
            Assert.IsTrue(result);
            _permissionApiServiceMock.Verify(p => p.GetAllPlantsForUserAsync(_currentUserOid), Times.Exactly(2));
        }

        [TestMethod]
        public async Task GetPermissionsForUser_ShouldReturnPermissionsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, _currentUserOid);

            // Assert
            AssertPermissions(result);
            _permissionApiServiceMock.Verify(p => p.GetPermissionsForCurrentUserAsync(Plant1IdWithAccess), Times.Once);
        }

        [TestMethod]
        public async Task GetPermissionsForUser_ShouldReturnPermissionsFromCacheSecondTime()
        {
            await _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, _currentUserOid);
            // Act
            var result = await _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, _currentUserOid);

            // Assert
            AssertPermissions(result);
            // since GetPermissionsForUserAsync has been called twice, but GetPermissionsAsync has been called once, the second Get uses cache
            _permissionApiServiceMock.Verify(p => p.GetPermissionsForCurrentUserAsync(Plant1IdWithAccess), Times.Once);
        }

        [TestMethod]
        public async Task GetProjectsForUserAsync_ShouldReturnProjectsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetProjectsForUserAsync(Plant1IdWithAccess, _currentUserOid);

            // Assert
            AssertProjects(result);
            _permissionApiServiceMock.Verify(p => p.GetAllOpenProjectsForCurrentUserAsync(Plant1IdWithAccess), Times.Once);
        }

        [TestMethod]
        public async Task GetProjectsForUserAsync_ShouldReturnProjectsFromCacheSecondTime()
        {
            await _dut.GetProjectsForUserAsync(Plant1IdWithAccess, _currentUserOid);
            // Act
            var result = await _dut.GetProjectsForUserAsync(Plant1IdWithAccess, _currentUserOid);

            // Assert
            AssertProjects(result);
            // since GetProjectsForUserAsync has been called twice, but GetProjectsAsync has been called once, the second Get uses cache
            _permissionApiServiceMock.Verify(p => p.GetAllOpenProjectsForCurrentUserAsync(Plant1IdWithAccess), Times.Once);
        }

        [TestMethod]
        public async Task GetContentRestrictionsForUserAsync_ShouldReturnPermissionsFromPermissionApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetContentRestrictionsForUserAsync(Plant1IdWithAccess, _currentUserOid);

            // Assert
            AssertRestrictions(result);
            _permissionApiServiceMock.Verify(p => p.GetContentRestrictionsForCurrentUserAsync(Plant1IdWithAccess), Times.Once);
        }

        [TestMethod]
        public async Task GetContentRestrictionsForUserAsync_ShouldReturnPermissionsFromCacheSecondTime()
        {
            await _dut.GetContentRestrictionsForUserAsync(Plant1IdWithAccess, _currentUserOid);
            // Act
            var result = await _dut.GetContentRestrictionsForUserAsync(Plant1IdWithAccess, _currentUserOid);

            // Assert
            AssertRestrictions(result);
            // since GetContentRestrictionsForUserAsync has been called twice, but GetContentRestrictionsAsync has been called once, the second Get uses cache
            _permissionApiServiceMock.Verify(p => p.GetContentRestrictionsForCurrentUserAsync(Plant1IdWithAccess), Times.Once);
        }

        [TestMethod]
        public async Task GetPermissionsForUser_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetPermissionsForUserAsync(Plant1IdWithAccess, Guid.Empty));

        [TestMethod]
        public async Task GetProjectsForUserAsync_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetProjectsForUserAsync(Plant1IdWithAccess, Guid.Empty));

        [TestMethod]
        public async Task GetContentRestrictionsForUserAsync_ShouldThrowExceptionWhenOidIsEmpty()
            => await Assert.ThrowsExceptionAsync<Exception>(() => _dut.GetContentRestrictionsForUserAsync(Plant1IdWithAccess, Guid.Empty));

        [TestMethod]
        public void ClearAll_ShouldClearAllPermissionCaches()
        {
            // Arrange
            var cacheManagerMock = new Mock<ICacheManager>();
            var dut = new PermissionCache(
                cacheManagerMock.Object,
                _currentUserProviderMock.Object,
                _permissionApiServiceMock.Object,
                new Mock<IOptionsMonitor<CacheOptions>>().Object);
            
            // Act
            dut.ClearAll(Plant1IdWithAccess, _currentUserOid);

            // Assert
            cacheManagerMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Exactly(4));
        }

        private void AssertPlants(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Plant1IdWithAccess, result.First());
            Assert.AreEqual(Plant2IdWithAccess, result.Last());
        }

        private void AssertPermissions(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Permission1, result.First());
            Assert.AreEqual(Permission2, result.Last());
        }

        private void AssertProjects(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Project1WithAccess, result.First());
            Assert.AreEqual(Project2WithAccess, result.Last());
        }

        private void AssertRestrictions(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Restriction1, result.First());
            Assert.AreEqual(Restriction2, result.Last());
        }
    }
}
