namespace Bfs.Iop.IndexSearch.Api.Hosting;

internal sealed class ScheduledReindexService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);

    private readonly ReindexOrchestrator _orchestrator;
    private readonly TimeProvider _time;
    private readonly ILogger<ScheduledReindexService> _logger;

    public ScheduledReindexService(
        ReindexOrchestrator orchestrator,
        TimeProvider time,
        ILogger<ScheduledReindexService> logger)
    {
        _orchestrator = orchestrator;
        _time = time;
        _logger = logger;
    }

    internal static DateTimeOffset NextRun(DateTimeOffset now)
    {
        var next = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero) + Offset;

        while (next <= now)
        {
            next += Interval;
        }

        return next;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        { 
            try
            {
                var now = _time.GetUtcNow();
                var due = NextRun(now);

                _logger.LogInformation("The next scheduled reindex is due at {Due:u}.", due);

                await Task.Delay(due - now, _time, stoppingToken); 

                var started = _orchestrator.TryStart();

                if (!started)
                {
                    _logger.LogInformation(
                        "Skipped the scheduled reindex due at {Due:u} because a pass was already "
                        + "running. The next slot will start one.",
                        due);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "A scheduled reindex slot failed. The schedule continues.");
            }
        }
    }
}
