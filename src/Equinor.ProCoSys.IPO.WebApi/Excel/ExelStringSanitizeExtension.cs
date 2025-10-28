using System.Text.RegularExpressions;

namespace Equinor.ProCoSys.IPO.WebApi.Excel;

public static partial class ExelStringSanitizeExtension
{
    [GeneratedRegex("|||")]
    private static partial Regex RegexExpression();

    public static string ExelSanitize(this string input)
    {
        if (input == null)
        {
            return null;
        }
        
        return RegexExpression().Replace(input, "");
    }
}
