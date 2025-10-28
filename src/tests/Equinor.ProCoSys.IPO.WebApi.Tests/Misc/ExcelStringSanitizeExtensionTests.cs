using Equinor.ProCoSys.IPO.WebApi.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Misc;

[TestClass]
public class ExcelStringSanitizeExtensionTests
{
    [TestMethod]
    [DataRow('')]
    [DataRow('')]
    [DataRow('')]
    [DataRow('')]
    public void ExcelSanitize_ShouldRemoveInvalidCharactersFromString(char invalidCharacter)
    {
        // Arrange
        var invalidString = $"Invalid {invalidCharacter} character string";
        const string Expected = "Invalid  character string";

        // Act
        var result = invalidString.ExcelSanitize();

        // Assert
        Assert.AreEqual(Expected, result);
    }
    
    [TestMethod]
    public void ExcelSanitize_ShouldRemoveMultipleInvalidCharactersFromString()
    {
        // Arrange
        const string InvalidString = "Multiple invalid character string";
        const string Expected = "Multiple invalid character string";

        // Act
        var result = InvalidString.ExcelSanitize();

        // Assert
        Assert.AreEqual(Expected, result);
    }
    
    [TestMethod]
    public void ExcelSanitize_ShouldHandleEmptyString()
    {
        // Act
        var result = string.Empty.ExcelSanitize();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }
    
    [TestMethod]
    public void ExcelSanitize_ShouldHandleNullString()
    {
        // Act
        var result = ((string)null).ExcelSanitize();

        // Assert
        Assert.IsNull(result);
    }
}
