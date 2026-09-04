using Bfs.Iop.DataAccess.Relational.Samples;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal sealed class IopDatabaseMigrator : IIopDatabaseMigrator
{
    private readonly IopDbContext _dbContext;

    public IopDatabaseMigrator(IopDbContext dbContext) =>
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task InsertSamplesAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Agents.Any(x => x.Id == AgentSamples.I14YTestId))
        {
            await _dbContext.Agents.AddRangeAsync(AgentSamples.Generate(), cancellationToken);
        }

        if (!_dbContext.IopPersons.Any(x => x.Id == IopPersonSamples.MaxMusterId))
        {
            await _dbContext.IopPersons.AddRangeAsync(IopPersonSamples.Generate(), cancellationToken);
        }

        if (!_dbContext.IopConcepts.Any())
        {
            await _dbContext.IopConcepts.AddRangeAsync(IopConceptSamples.Generate(), cancellationToken);
            await _dbContext.VocabularyConfigs.AddRangeAsync(VocabularyConfigSamples.Generate(), cancellationToken);
        }

        if (!_dbContext.Datasets.Any())
        {
            await _dbContext.Datasets.AddRangeAsync(DatasetSamples.Generate(), cancellationToken);
        }

        if (!_dbContext.PublicServices.Any())
        {
            await _dbContext.PublicServices.AddRangeAsync(PublicServiceSamples.Generate(), cancellationToken);
        }

        if (!_dbContext.DataServices.Any())
        {
            await _dbContext.DataServices.AddRangeAsync(DataServiceSamples.Generate(), cancellationToken);
        }

        if (!_dbContext.MappingTables.Any())
        {
            await _dbContext.MappingTables.AddRangeAsync(MappingTableSamples.Generate(), cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task MigrateAsync(CancellationToken cancellationToken) =>
        _dbContext.Database.MigrateAsync(cancellationToken);
}