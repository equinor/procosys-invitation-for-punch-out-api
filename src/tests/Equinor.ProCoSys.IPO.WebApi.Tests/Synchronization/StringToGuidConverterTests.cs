using System;
using System.Text.Json;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            // Act & Assert
            JsonSerializer.Deserialize<Guid>(input, _options);
        }

        [TestMethod]
        public void Read_WithValidNonHyphenatedString_ReturnsGuidValue()
        {
            // Arrange
            var expectedGuid = new Guid("20964082-0381-4F9C-E063-0A14000A02B8");
            var input = "\"2096408203814F9CE0630A14000A02B8\""; // Non-hyphenated JSON

            // Act
            var resultGuid = JsonSerializer.Deserialize<Guid>(input, _options);

            // Assert
            Assert.AreEqual(expectedGuid, resultGuid);
        }

        [TestMethod]
        public void Read_WithValidHyphenatedString_ReturnsGuidValue()
        {
            // Arrange
            var expectedGuid = new Guid("00000000-2222-2222-2222-333333333321");
            var input = "\"00000000-2222-2222-2222-333333333321\""; // Hyphenated JSON

            // Act
            var resultGuid = JsonSerializer.Deserialize<Guid>(input, _options);

            // Assert
            Assert.AreEqual(expectedGuid, resultGuid);
        }
    }
}
