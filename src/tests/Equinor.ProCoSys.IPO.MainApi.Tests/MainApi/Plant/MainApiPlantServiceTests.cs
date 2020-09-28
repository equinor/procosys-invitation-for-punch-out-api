using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.ForeignApi.Tests.MainApi.Plant
{
    [TestClass]
    public class MainApiPlantServiceTests
    {
        private MainApiPlantService _dut;
        private readonly string _plantId = "PCS$AASTA_HANSTEEN";
        private readonly string _plantTitle = "AastaHansteen";

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            var mainApiOptions = new Mock<IOptionsMonitor<MainApiOptions>>();
            mainApiOptions
                .Setup(x => x.CurrentValue)
                .Returns(new MainApiOptions { ApiVersion = "4.0", BaseAddress = "http://example.com" });
            var mainApiClient = new Mock<IBearerTokenApiClient>();
            mainApiClient
                .Setup(x => x.QueryAndDeserializeAsync<List<ProCoSysPlant>>(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new List<ProCoSysPlant>
                {
                    new ProCoSysPlant { Id = _plantId, Title = _plantTitle },
                    new ProCoSysPlant { Id = "PCS$ASGARD", Title = "Åsgard" },
                    new ProCoSysPlant { Id = "PCS$ASGARD_A", Title = "ÅsgardA" },
                    new ProCoSysPlant { Id = "PCS$ASGARD_B", Title = "ÅsgardB" },
                }));

            _dut = new MainApiPlantService(mainApiClient.Object, mainApiOptions.Object);
        }

        [TestMethod]
        public async Task GetPlants_ShouldReturnCorrectNumberOfPlants()
        {
            // Act
            var result = await _dut.GetPlantsAsync();

            // Assert
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public async Task GetPlants_ShouldSetsCorrectProperties()
        {
            // Act
            var result = await _dut.GetPlantsAsync();

            // Assert
            var plant = result.First();
            Assert.AreEqual(_plantId, plant.Id);
            Assert.AreEqual(_plantTitle, plant.Title);
        }
    }
}
