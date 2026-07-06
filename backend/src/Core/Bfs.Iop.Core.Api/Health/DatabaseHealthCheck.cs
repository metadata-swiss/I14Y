using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private DbContext _dbContext;

    public DatabaseHealthCheck(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.OpenConnectionAsync(cancellationToken);
            using var connection = _dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
