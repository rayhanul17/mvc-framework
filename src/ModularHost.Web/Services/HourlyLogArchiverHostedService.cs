using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Infrastructure;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MRCMS.Services
{
    public class HourlyLogArchiverHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HourlyLogArchiverHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Timer _timer;

        public HourlyLogArchiverHostedService(
            IServiceProvider serviceProvider,
            ILogger<HourlyLogArchiverHostedService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for initial delay to let the application start properly
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Application is shutting down before the initial delay
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ArchiveLogs();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error archiving logs");
                }

                try
                {
                    // Wait for 1 hour
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // This is expected when the application is shutting down
                    break;
                }
            }
        }

        private async Task ArchiveLogs()
        {
            var threshold = DateTime.UtcNow.AddHours(-1);
            var useMongo = _configuration.GetValue<bool>("Audit:UseMongo");

            if (useMongo)
            {
                await ArchiveMongoLogs(threshold);
            }
            else
            {
                await ArchiveMySqlLogs(threshold);
            }
        }

        private async Task ArchiveMySqlLogs(DateTime threshold)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var logsToArchive = await context.Logs
                    .Where(l => l.CreatedAt < threshold)
                    .ToListAsync();

                if (logsToArchive.Any())
                {
                    var archives = logsToArchive.Select(log => new LogArchive
                    {
                        Id = log.Id,
                        Level = log.Level,
                        Message = log.Message,
                        Exception = log.Exception,
                        Properties = log.Properties,
                        UserId = log.UserId,
                        UserName = log.UserName,
                        Url = log.Url,
                        HttpMethod = log.HttpMethod,
                        IpAddress = log.IpAddress,
                        UserAgent = log.UserAgent,
                        CreatedAt = log.CreatedAt,
                        MachineName = log.MachineName,
                        Application = log.Application,
                        ArchivedAt = DateTime.UtcNow
                    }).ToList();

                    await context.LogArchives.AddRangeAsync(archives);
                    context.Logs.RemoveRange(logsToArchive);
                    
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation($"Archived {logsToArchive.Count} logs");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during MySQL log archiving");
                throw;
            }
        }

        private async Task ArchiveMongoLogs(DateTime threshold)
        {
            var connectionString = _configuration.GetValue<string>("Audit:MongoConnection");
            var databaseName = _configuration.GetValue<string>("Audit:MongoDatabase");
            
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            var logsCollection = database.GetCollection<Log>("logs");
            var archiveCollection = database.GetCollection<LogArchive>("logs_archive");

            var filter = Builders<Log>.Filter.Lt(l => l.CreatedAt, threshold);
            var logs = await logsCollection.Find(filter).ToListAsync();

            if (logs.Any())
            {
                var archives = logs.Select(log => new LogArchive
                {
                    Id = log.Id,
                    Level = log.Level,
                    Message = log.Message,
                    Exception = log.Exception,
                    Properties = log.Properties,
                    UserId = log.UserId,
                    UserName = log.UserName,
                    Url = log.Url,
                    HttpMethod = log.HttpMethod,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    CreatedAt = log.CreatedAt,
                    MachineName = log.MachineName,
                    Application = log.Application,
                    ArchivedAt = DateTime.UtcNow
                }).ToList();

                await archiveCollection.InsertManyAsync(archives);
                await logsCollection.DeleteManyAsync(filter);
                
                _logger.LogInformation($"Archived {logs.Count} logs from MongoDB");
            }
        }
    }
}