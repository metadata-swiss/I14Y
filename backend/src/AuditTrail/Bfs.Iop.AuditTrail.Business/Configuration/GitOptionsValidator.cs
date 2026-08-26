using Microsoft.Extensions.Options;

namespace Bfs.Iop.AuditTrail.Business.Configuration;

internal sealed class GitOptionsValidator : IValidateOptions<GitOptions>
{
    public ValidateOptionsResult Validate(string? name, GitOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.RepositoryPath))
        {
            return ValidateOptionsResult.Fail(
                "'RepositoryPath' must be specified.");
        }

        if (!Directory.Exists(options.RepositoryPath))
        {
            return ValidateOptionsResult.Fail(
                $"RepositoryPath '{options.RepositoryPath}' does not exist.");
        }

        return ValidateOptionsResult.Success;
    }
}
