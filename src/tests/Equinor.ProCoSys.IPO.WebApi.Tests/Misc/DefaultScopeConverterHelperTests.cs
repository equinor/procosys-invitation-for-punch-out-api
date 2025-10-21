using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Equinor.ProCoSys.IPO.WebApi.Authorizations.DefaultScopeConverterHelper;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Misc;

[TestClass]
public class DefaultScopeConverterHelperTests
{
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void TryConvertToDefaultScope_ShouldReturnFalse_WhenNoScopeIsProvided(string scope)
    {
        // Act
        var result = TryConvertToDefaultScope(scope, out _);

        // Assert
        Assert.IsFalse(result);
    }
    
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void TryConvertToDefaultScope_ShouldOut_Empty_WhenNoScopeIsProvided(string scope)
    {
        // Act
        TryConvertToDefaultScope(scope, out var defaultScope);

        // Assert
        Assert.AreEqual(string.Empty, defaultScope);
    }
    
    [TestMethod]
    public void TryConvertToDefaultScope_ShouldReturnFalse_WhenInvalidGuidIsProvided()
    {
        // Arrange
        var scope = "invalid-guid";

        // Act
        var result = TryConvertToDefaultScope(scope, out _);

        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public void TryConvertToDefaultScope_ShouldOutEmpty_WhenInvalidGuidIsProvided()
    {
        // Arrange
        var scope = "invalid-guid";

        // Act
        TryConvertToDefaultScope(scope, out var defaultScope);

        // Assert
        Assert.AreEqual(string.Empty, defaultScope);
    }
}
