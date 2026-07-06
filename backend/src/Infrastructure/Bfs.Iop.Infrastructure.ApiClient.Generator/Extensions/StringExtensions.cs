namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// Converts a string to upper camel case
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    internal static string ToUpperCamelCase(this string text) => 
        string.IsNullOrWhiteSpace(text) 
            ? text :
            $"{text[..1].ToUpper()}{text[1..]}";

    /// <summary>
    /// Converts all line endings in this <paramref name="input"/> to CRLF
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static string WithCrlfLineEndings(this string input)
    {
        input = input?.Replace("\r", "") ?? "";
        return input.Replace("\n", "\r\n");
    }
}
