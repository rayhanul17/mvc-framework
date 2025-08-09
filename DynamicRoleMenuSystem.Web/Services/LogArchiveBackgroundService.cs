using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Common;

namespace DynamicRoleMenuSystem.Web.Services;

public class LogArchiveBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogArchiveBackgroundService> _logger;
    private readonly AuditLogSettings _auditLogSettings;
    private readonly TimeSpan _checkInterval;
    
    public LogArchiveBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<LogArchiveBackgroundService> logger,
        IOptions<AppSettings> appSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _auditLogSettings = appSettings.Value.AuditLog;
        
        // Check every hour for logs to archive
        _checkInterval = TimeSpan.FromHours(1);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_auditLogSettings.Enabled)
        {
            _logger.LogInformation("Audit log archiving is disabled");
            return;
        }
        
        _logger.LogInformation("Log Archive Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                    
                    // Archive old logs
                    var archiveResult = await logService.ArchiveOldLogsAsync();
                    if (archiveResult.IsSuccess)
                    {
                        _logger.LogInformation("Successfully archived old logs");
                    }
                    else
                    {
                        _logger.LogError($"Failed to archive logs: {archiveResult.ErrorMessage}");
                    }
                    
                    // Cleanup very old archived logs
                    var cleanupResult = await logService.CleanupArchivedLogsAsync();
                    if (cleanupResult.IsSuccess)
                    {
                        _logger.LogInformation("Successfully cleaned up old archived logs");
                    }
                    else
                    {
                        _logger.LogError($"Failed to cleanup archived logs: {cleanupResult.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing log archives");
            }
        }
        
        _logger.LogInformation("Log Archive Background Service stopped");
    }
}