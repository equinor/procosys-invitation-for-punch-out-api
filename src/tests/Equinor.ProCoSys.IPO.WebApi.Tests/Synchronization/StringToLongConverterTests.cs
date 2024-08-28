using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Synchronization
{

    [TestClass]
    public class StringToLongConverterTests
    {
        private StringToLongConverter _converter;
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new StringToLongConverter();
            _options = new JsonSerializerOptions();
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException),
            "Unable to deserialize JSON. Cannot convert invalid_string to System.Int64.")]
        public void Read_WithInvalidString_ThrowsJsonException()
        {
            // Arrange
            var input = "\"invalid_string\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(input));
            reader.Read(); 

            // Act
            _converter.Read(ref reader, typeof(long), _options);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void Read_WithValidString_ReturnsLongValue()
        {
            // Arrange
            var expectedValue = 1234567890L;
            var input = $"\"{expectedValue}\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(input));
            reader.Read();

            // Act
            var result = _converter.Read(ref reader, typeof(long), _options);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }
    }
}
