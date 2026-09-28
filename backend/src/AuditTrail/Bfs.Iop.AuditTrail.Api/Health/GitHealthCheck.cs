using Bfs.Iop.AuditTrail.Business.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bfs.Iop.AuditTrail.Api.Health;

public sealed class GitHealthCheck : IHealthCheck
{
    private readonly IGitWrapper _gitWrapper;

    public GitHealthCheck(IGitWrapper gitWrapper) => _gitWrapper = gitWrapper;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
		try
		{
            var response = await _gitWrapper.ExecuteAsync(["--version"], cancellationToken);

            if (response.ExitCode is 0)
            {
                return HealthCheckResult.Healthy(response.StdOut);
            }
            else
            {
                return HealthCheckResult.Degraded($"Git command failed with exit code {response.ExitCode}. StdErr: {response.StdErr}");
            }
        }
		catch (Exception ex)
		{
            return HealthCheckResult.Unhealthy("Git is not installed or cannot be executed.", exception: ex);
        }
    }
}
