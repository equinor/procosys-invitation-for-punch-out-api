using System.Text.RegularExpressions;

namespace Equinor.ProCoSys.IPO.WebApi.Excel;

public static partial class ExcelStringSanitizeExtension
{
    [GeneratedRegex("|||")]
    private static partial Regex RegexExpression();

    public static string ExcelSanitize(this string input)
    {
        if (input == null)
        {
            return null;
        }

        return RegexExpression().Replace(input, "");
    }
}
