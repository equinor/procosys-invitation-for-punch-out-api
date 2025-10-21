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
    [DataRow("invalid-guid")]
    [DataRow("www.procosys.com")]
    [DataRow("www.procosys.com/something")]
    public void TryConvertToDefaultScope_ShouldReturn_False_WhenProvidedScopeIs(string scope)
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
    [DataRow("invalid-guid")]
    [DataRow("www.procosys.com")]
    [DataRow("www.procosys.com/something")]
    public void TryConvertToDefaultScope_ShouldOut_Empty_WhenProvidedScopeIs(string scope)
    {
        // Act
        TryConvertToDefaultScope(scope, out var defaultScope);

        // Assert
        Assert.AreEqual(string.Empty, defaultScope);
    }

    [DataTestMethod]
    [DataRow("123e4567-e89b-12d3-a456-426614174000")]
    [DataRow("http://www.foo.bar")]
    [DataRow("https://www.foo.bar")]
    [DataRow("https://www.foo.bar/")]
    public void TryConvertToDefaultScope_ShouldReturn_True_WhenProvidedScopeIs(string scope)
    {
        // Act
        var result = TryConvertToDefaultScope(scope, out _);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [DataTestMethod]
    [DataRow("123e4567-e89b-12d3-a456-426614174000", "123e4567-e89b-12d3-a456-426614174000/.default")]
    [DataRow("http://www.foo.bar", "http://www.foo.bar/.default")]
    [DataRow("https://www.foo.bar", "https://www.foo.bar/.default")]
    [DataRow("https://www.foo.bar/", "https://www.foo.bar/.default")]
    public void TryConvertToDefaultScope_ShouldReturn_ExpectedDefaultScope_WhenProvidedScopeIs(string scope, string expected)
    {
        // Act
        TryConvertToDefaultScope(scope, out var result);
        
        // Assert
       Assert.AreEqual(expected, result);
    }
}
