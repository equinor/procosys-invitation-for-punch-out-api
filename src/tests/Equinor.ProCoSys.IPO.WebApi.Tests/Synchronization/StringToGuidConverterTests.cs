using System;
using System.Globalization;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Synchronization
{

    [TestClass]
    public class StringToGuidConverterTests
    {
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void TestInitialize()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new StringToGuidConverter() }
            };
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Read_WithInvalidString_ThrowsJsonException()
        {
            // Arrange
            var input = "\"not_a_valid_guid\"";

            // Act
            var ex = Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<Guid>(input, _options));

            // Assert
            StringAssert.Contains(ex.Message, "The JSON value is not in a guid format:");
        }

        [TestMethod]
        public void Read_WithValidString_ReturnsGuidValue()
        {
            // Arrange
            var expectedGuid = new Guid("20964082-0381-4F9C-E063-0A14000A02B8"); // With hyphens for Guid parsing
            var input = "\"2096408203814F9CE0630A14000A02B8\""; // Without hyphens in JSON

            // Act
            var resultGuid = JsonSerializer.Deserialize<Guid>(input, _options);

            // Assert
            Assert.AreEqual(expectedGuid, resultGuid);
        }
    }
}
