using System.Text.RegularExpressions;

namespace Equinor.ProCoSys.IPO.Command;

public static class StringExtensions
{
    public static string StripHtml(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
