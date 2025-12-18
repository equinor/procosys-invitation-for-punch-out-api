using System;
using System.Globalization;
using System.Text.Json;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Synchronization
{

    [TestClass]
    public class StringToDateTimeConverterTests
    {
        private StringToDateTimeConverter _converter;
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new StringToDateTimeConverter();
            _options = new JsonSerializerOptions();
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Read_WithInvalidString_ThrowsJsonException()
        {
            // Arrange
            var input = "\"invalid_datetime\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(input));
            reader.Read(); // Read first token

            // Act
            var result = _converter.Read(ref reader, typeof(DateTime), _options);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void Read_WithValidString_ReturnsDateTimeValue()
        {
            // Arrange
            var expectedValue = DateTime.ParseExact("2024-08-26 12:27:44", "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture);
            var input = $"\"{expectedValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}\"";
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(input));
            reader.Read(); // Read first token

            // Act
            var result = _converter.Read(ref reader, typeof(DateTime), _options);

            // Assert
            Assert.AreEqual(expectedValue, result);
        }
    }
}
