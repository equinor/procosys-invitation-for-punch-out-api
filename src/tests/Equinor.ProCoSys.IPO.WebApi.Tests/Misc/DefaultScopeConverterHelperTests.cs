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
        var result = TryConvertToDefaultScope(scope, out var defaultScope);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(string.Empty, defaultScope);
    }
}
