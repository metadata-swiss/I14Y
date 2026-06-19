namespace Bfs.Iop.Core.Data.Contracts;

public interface IIopDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);

    Task InsertSamplesAsync(CancellationToken cancellationToken);
}
