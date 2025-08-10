using System.Diagnostics;

namespace DynamicRoleMenuSystem.Web.Services;

public class HeartbeatService : BackgroundService
{
    private readonly ILogger<HeartbeatService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _timer;
    private int _heartbeatCounter = 0;

    public HeartbeatService(ILogger<HeartbeatService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        // Create a timer that triggers every 5 minutes
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Log initial heartbeat on startup
        LogHeartbeat();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _timer.WaitForNextTickAsync(stoppingToken);
                LogHeartbeat();
            }
            catch (OperationCanceledException)
            {
                // This is expected when the service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during heartbeat logging");
            }
        }
    }

    private void LogHeartbeat()
    {
        _heartbeatCounter++;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
            
            // Get system information
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64 / (1024 * 1024); // Convert to MB
            var threadCount = process.Threads.Count;
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
            
            // Get additional application metrics
            var gcMemory = GC.GetTotalMemory(false) / (1024 * 1024); // Convert to MB
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);
            
            // Write to heartbeat log file directly using Serilog
            var heartbeatLogger = Serilog.Log.ForContext("SourceContext", "Heartbeat");
            heartbeatLogger.Information(
                "Application Heartbeat #{HeartbeatNumber} | " +
                "Status: Healthy | " +
                "Uptime: {Uptime:hh\\:mm\\:ss} | " +
                "Memory: {MemoryMB} MB | " +
                "GC Memory: {GCMemoryMB} MB | " +
                "Threads: {ThreadCount} | " +
                "GC Gen0: {Gen0} Gen1: {Gen1} Gen2: {Gen2} | " +
                "Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss}",
                _heartbeatCounter,
                uptime,
                memoryUsage,
                gcMemory,
                threadCount,
                gen0Collections,
                gen1Collections,
                gen2Collections,
                DateTime.UtcNow
            );
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}