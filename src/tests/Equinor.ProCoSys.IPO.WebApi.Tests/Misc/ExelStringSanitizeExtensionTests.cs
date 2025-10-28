using Equinor.ProCoSys.IPO.WebApi.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Misc;

[TestClass]
public class ExelStringSanitizeExtensionTests
{
    [TestMethod]
    [DataRow('')]
    [DataRow('')]
    [DataRow('')]
    [DataRow('')]
    public void ExelSanitize_ShouldRemoveInvalidCharactersFromString(char invalidCharacter)
    {
        // Arrange
        var invalidString = $"Invalid {invalidCharacter} character string";
        const string Expected = "Invalid  character string";

        // Act
        var result = invalidString.ExelSanitize();

        // Assert
        Assert.AreEqual(Expected, result);
    }
    
    [TestMethod]
    public void ExelSanitize_ShouldRemoveMultipleInvalidCharactersFromString()
    {
        // Arrange
        const string InvalidString = "Multiple invalid character string";
        const string Expected = "Multiple invalid character string";

        // Act
        var result = InvalidString.ExelSanitize();

        // Assert
        Assert.AreEqual(Expected, result);
    }
    
    [TestMethod]
    public void ExelSanitize_ShouldHandleEmptyString()
    {
        // Act
        var result = string.Empty.ExelSanitize();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }
    
    [TestMethod]
    public void ExelSanitize_ShouldHandleNullString()
    {
        // Act
        var result = ((string)null).ExelSanitize();

        // Assert
        Assert.IsNull(result);
    }
}
