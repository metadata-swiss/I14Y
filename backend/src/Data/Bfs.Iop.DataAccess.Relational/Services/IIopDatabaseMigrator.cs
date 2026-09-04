namespace Bfs.Iop.DataAccess.Relational.Services;

public interface IIopDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);

    Task InsertSamplesAsync(CancellationToken cancellationToken);
}
