using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    [DataRow("<p>This is a paragraph.</p>", "This is a paragraph.")]
    [DataRow("<div><p>Nested tags</p></div>", "Nested tags")]
    [DataRow("<a href='https://example.com'>Link</a>", "Link")]
    [DataRow("<b>Bold</b> and <i>Italic</i>", "Bold and Italic")]
    [DataRow("", "")]
    [DataRow(null, "")]
    [DataRow("No HTML tags", "No HTML tags")]
    public void StripHtml_RemovesHtmlTags(string input, string expected)
    {
        // Act
        var result = input.StripHtml();

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("<p>This is a paragraph.</p>", true)]
    [DataRow("<div><p>Nested tags</p></div>", true)]
    [DataRow("<a href='https://example.com'>Link</a>", true)]
    [DataRow("<b>Bold</b> and <i>Italic</i>", true)]
    [DataRow("", false)]
    [DataRow(null, false)]
    [DataRow("No HTML tags", false)]
    public void ContainsHtml_DetectsHtmlTags(string input, bool expected)
    {
        // Act
        var result = input.ContainsHtml();

        // Assert
        Assert.AreEqual(expected, result);
    }
}
