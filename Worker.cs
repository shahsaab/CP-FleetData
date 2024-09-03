namespace CP_FleetDataJob;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMyFileWatcher _watcher;

    public Worker(ILogger<Worker> logger,IMyFileWatcher watcher)
    {
        _logger = logger;
        _watcher = watcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Run(() => _watcher.Start());
    }
}
