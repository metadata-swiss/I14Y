using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Iri.Api.Extensions;

public static class WebControllerExtensions
{
    public static string NegotiateLanguage(this HttpRequest request, I14YOptions i14yOptions)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(i14yOptions);

        var defaultLanguage = i14yOptions.DefaultLanguage.ToString().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(request.Headers.AcceptLanguage))
        {
            return defaultLanguage;
        }

        foreach (var part in request.Headers.AcceptLanguage.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var tag = part.Trim().Split(';')[0].Trim().ToLowerInvariant(); // de-ch
            var primary = tag.Split('-')[0]; // de

            if (Enum.TryParse<Language>(
                    primary,
                    ignoreCase: true,
                    out var language)
                && Enum.IsDefined(language))
            {
                return language.ToString().ToLowerInvariant();
            }
        }

        return defaultLanguage;
    }

    public static bool AcceptsContentTypes(this HttpRequest request, string[] AcceptedContentTypes)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (AcceptedContentTypes.Contains("*/*") && string.IsNullOrWhiteSpace(request.Headers.Accept))
        {
            // Because we accept "*/*"
            return true;
        }

        return AcceptedContentTypes.Any(i =>
            request.Headers.Accept.ToString()
                .Contains(i, StringComparison.OrdinalIgnoreCase));
    }

    public static ObjectResult UnsupportedMediaType(this ControllerBase controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        return controller.Problem(
                        statusCode: StatusCodes.Status415UnsupportedMediaType,
                        title: "Unsupported 'Accept' header.",
                        detail: "This endpoint cannot produce any of the requested media types.");
    }

    public static IActionResult RedirectOrResolveJson(this ControllerBase controller, string target, bool resolveOnly)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

        controller.Response.Headers.Vary = "Accept, Accept-Language";

        if (resolveOnly || (controller.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)))
        {
            controller.Response.Headers.Location = target;
            return controller.Ok(new { location = target, status = StatusCodes.Status302Found });
        }

        return controller.Redirect(target);
    }

    public static bool IsValidVersion(this string version)
    {
        string versionRegexPattern = "^(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-((?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$";

        var isMatch=Regex.IsMatch(version, versionRegexPattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

        return !string.IsNullOrWhiteSpace(version) && isMatch;
    }
}