using System.Text.RegularExpressions;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;

internal static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        Regex startUnderscoresRegex = new(@"^_+");
        Regex snakeCaseReplaceRegex = new(@"([a-z0-9])([A-Z])");

        var startUnderscores = startUnderscoresRegex.Match(input);

        var snakeCased = startUnderscores + snakeCaseReplaceRegex.Replace(input, "$1_$2");

        return snakeCased.ToLower();
    }
}
