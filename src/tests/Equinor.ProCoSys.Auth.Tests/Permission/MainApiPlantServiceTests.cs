using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Permission;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Auth.Tests.Permission
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
            var mainApiClient = new Mock<IMainApiClient>();
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
        public async Task GetAllPlants_ShouldReturnCorrectNumberOfPlants()
        {
            // Act
            var result = await _dut.GetAllPlantsAsync();

            // Assert
            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public async Task GetAllPlants_ShouldSetsCorrectProperties()
        {
            // Act
            var result = await _dut.GetAllPlantsAsync();

            // Assert
            var plant = result.First();
            Assert.AreEqual(_plantId, plant.Id);
            Assert.AreEqual(_plantTitle, plant.Title);
        }
    }
}
