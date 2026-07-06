using Bfs.Iop.Core.Lucene.IndexBuilders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Lucene;

internal class LuceneHostedService : IHostedService
{
    private readonly LuceneIndexBuilderService _builder;
    private readonly ILogger<LuceneHostedService> _logger;

    public LuceneHostedService(LuceneIndexBuilderService builder, ILogger<LuceneHostedService> logger)
    {
        _builder = builder;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Lucene index build in background...");

        _ = Task.Run(async () =>
        {
            try
            {
                await _builder.RebuildIndex(cancellationToken);
                _logger.LogInformation("Lucene index built successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build Lucene index.");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
