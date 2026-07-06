using FluentValidation;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Core.Validation.Extensions;

internal static class StringValidationExtensions
{
    private const char WhiteSpace = ' ';

    public static IRuleBuilderOptions<T, string> MustBeValidIdentifier<T>(
        this IRuleBuilder<T, string> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .Must(IsValidIdentifier)
            .WithMessage((_, str) => $"The value '{str}' is not a valid identifier.");           
    }

    public static bool IsNullOrEmptyOrContainsWhiteSpace(this string? str) =>
        string.IsNullOrEmpty(str) || str.Contains(WhiteSpace);

    public static bool IsValidIdentifier(this string identifier)
    {
        if (IsNullOrEmptyOrContainsWhiteSpace(identifier))
        {
            return false;
        }

        Regex regex = new("^[A-Za-z0-9._~@:-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return regex.IsMatch(identifier);
    }

    public static bool IsValidUri(this string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
        {
            return false;
        }

        string pattern = @"^(?:(https?|ftp):\/\/(?:[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}|(?:\[[^\]]+\]))(?:\:[0-9]+)?(?:\/[^\s?#]*)?(?:\?[^#\s]*)?(?:\#[^\s]*)?|ldap:\/\/(?:\[[^\]]+\]|[^\/\s:?#]+)(?:\:[0-9]+)?(?:\/[^\s?#]*)?((?:\?[^#\s]*)?(?:\#[^\s]*)?)|mailto:[^\s?#]+|news:[^\s?#]+|tel:[^\s?#]+|telnet:\/\/(?:\[[^\]]+\]|[^\/\s:?#]+)(?:\:[0-9]+)?(?:\/[^\s?#]*)?(?:\?[^#\s]*)?(?:\#[^\s]*)?|urn:[a-zA-Z0-9][a-zA-Z0-9-]{1,31}:(?:[a-zA-Z0-9()+,\-.:=@;$_!*'%/?#]+))$";
        Regex regex = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return regex.IsMatch(uriString);
    }

    public static bool IsValidEmail(this string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var pattern = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return regex.IsMatch(email);
    }

    public static bool IsValidVersion(this string version)
    {
        string versionRegexPattern = "^(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-((?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$";
        Regex _versionFormatRegex = new(versionRegexPattern, RegexOptions.Compiled);

        return !string.IsNullOrWhiteSpace(version) && _versionFormatRegex.IsMatch(version);
    }

    public static bool IsValidFileExtension(this string fileExtension, IEnumerable<string> validExtensions)
    {
        ArgumentNullException.ThrowIfNull(validExtensions, nameof(validExtensions));

        return !string.IsNullOrWhiteSpace(fileExtension) && validExtensions.Contains(fileExtension);
    }

    public static bool IsValidUid(this string uid)
    {
        var uidLength = 12;
        var uidPattern = "CHE[1-9][0-9]{8}";
        var regex = new Regex(uidPattern);

        return !string.IsNullOrWhiteSpace(uid) && uid.Length == uidLength && regex.IsMatch(uid);
    }
}
