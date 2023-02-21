﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Time;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Caching
{
    [TestClass]
    public class PlantCacheTests
    {
        private readonly Guid _currentUserOid = new Guid("12345678-1234-1234-1234-123456789123");
        private Mock<IPlantApiService> _plantApiServiceMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private readonly string Plant1WithAccess = "P1";
        private readonly string Plant2WithAccess = "P2";
        private readonly string PlantWithoutAccess = "P3";

        private PlantCache _dut;

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            var optionsMock = new Mock<IOptionsMonitor<CacheOptions>>();
            optionsMock
                .Setup(x => x.CurrentValue)
                .Returns(new CacheOptions());

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock.Setup(c => c.GetCurrentUserOid()).Returns(_currentUserOid);

            _plantApiServiceMock = new Mock<IPlantApiService>();
            _plantApiServiceMock.Setup(p => p.GetAllPlantsAsync()).Returns(Task.FromResult(
                new List<ProCoSysPlant>
                {
                    new ProCoSysPlant {Id = Plant1WithAccess, HasAccess = true}, 
                    new ProCoSysPlant {Id = Plant2WithAccess, HasAccess = true},
                    new ProCoSysPlant {Id = PlantWithoutAccess}
                }));

            _dut = new PlantCache(new CacheManager(), _currentUserProviderMock.Object, _plantApiServiceMock.Object, optionsMock.Object);
        }

        [TestMethod]
        public async Task GetPlantWithAccessForUserAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            var result = await _dut.GetPlantWithAccessForUserAsync(_currentUserOid);

            // Assert
            AssertPlants(result);
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetPlantWithAccessForUserAsync_ShouldReturnPlantsFromCacheSecondTime()
        {
            await _dut.GetPlantWithAccessForUserAsync(_currentUserOid);

            // Act
            var result = await _dut.GetPlantWithAccessForUserAsync(_currentUserOid);

            // Assert
            AssertPlants(result);
            // since GetPlantWithAccessForUserAsyncAsync has been called twice, but GetPlantsAsync has been called once, the second Get uses cache
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnTrue_WhenKnownPlant()
        {
            // Act
            var result = await _dut.HasCurrentUserAccessToPlantAsync(Plant2WithAccess);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.HasCurrentUserAccessToPlantAsync("XYZ");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            await _dut.HasCurrentUserAccessToPlantAsync(Plant2WithAccess);

            // Assert
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task HasCurrentUserAccessToPlantAsync_ShouldReturnPlantsFromCacheSecondTime()
        {
            await _dut.HasCurrentUserAccessToPlantAsync("XYZ");
            // Act
            await _dut.HasCurrentUserAccessToPlantAsync(Plant2WithAccess);

            // Assert
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnTrue_WhenKnownPlant()
        {
            // Act
            var result = await _dut.HasUserAccessToPlantAsync(Plant2WithAccess, _currentUserOid);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnFalse_WhenUnknownPlant()
        {
            // Act
            var result = await _dut.HasUserAccessToPlantAsync("XYZ", _currentUserOid);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnPlantIdsFromPlantApiServiceFirstTime()
        {
            // Act
            await _dut.HasUserAccessToPlantAsync(Plant2WithAccess, _currentUserOid);

            // Assert
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task HasUserAccessToPlantAsync_ShouldReturnPlantsFromCache()
        {
            await _dut.HasUserAccessToPlantAsync("ABC", _currentUserOid);
            // Act
            await _dut.HasUserAccessToPlantAsync(Plant2WithAccess, _currentUserOid);

            // Assert
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);
        }

        [TestMethod]
        public async Task IsAValidPlant_ShouldReturnTrue_WhenKnownPlantWithAccess()
        {
            // Act
            var result = await _dut.IsAValidPlantAsync(Plant2WithAccess);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsAValidPlant_ShouldReturnTrue_WhenKnownPlantWithoutAccess()
        {
            // Act
            var result = await _dut.IsAValidPlantAsync(PlantWithoutAccess);

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
        public async Task Clear_ShouldForceGettingPlantsFromApiServiceAgain()
        {
            // Arrange
            var result = await _dut.HasUserAccessToPlantAsync(Plant2WithAccess, _currentUserOid);
            Assert.IsTrue(result);
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Once);

            // Act
            _dut.Clear(_currentUserOid);

            // Assert
            result = await _dut.HasUserAccessToPlantAsync(Plant2WithAccess, _currentUserOid);
            Assert.IsTrue(result);
            _plantApiServiceMock.Verify(p => p.GetAllPlantsAsync(), Times.Exactly(2));
        }

        private void AssertPlants(IList<string> result)
        {
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Plant1WithAccess, result.First());
            Assert.AreEqual(Plant2WithAccess, result.Last());
        }
    }
}
