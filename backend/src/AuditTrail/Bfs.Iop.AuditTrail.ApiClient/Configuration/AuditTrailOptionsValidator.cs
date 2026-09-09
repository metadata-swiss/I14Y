using Microsoft.Extensions.Options;

namespace Bfs.Iop.AuditTrail.ApiClient.Configuration;

internal sealed class AuditTrailOptionsValidator : IValidateOptions<AuditTrailOptions>
{
    public ValidateOptionsResult Validate(string? name, AuditTrailOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return ValidateOptionsResult.Fail($"{nameof(AuditTrailOptions.BaseUrl)} is required.");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail($"{nameof(AuditTrailOptions.BaseUrl)} must be a valid absolute URI.");
        }

        return ValidateOptionsResult.Success;
    }
}
