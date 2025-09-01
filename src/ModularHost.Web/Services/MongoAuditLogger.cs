using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MRCMS.Core.Models.Entities;
using MRCMS.Services.Interfaces;
using MRCMS.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MRCMS.Services
{
    public class MongoAuditLogger : IAuditLogger
    {
        private readonly IMongoCollection<Log> _logsCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MongoAuditLogger(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
            var connectionString = configuration.GetValue<string>("Audit:MongoConnection");
            var databaseName = configuration.GetValue<string>("Audit:MongoDatabase");
            
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _logsCollection = database.GetCollection<Log>("logs");
        }

        public async Task LogAsync(Log log)
        {
            EnrichLogWithContext(log);
            await _logsCollection.InsertOneAsync(log);
        }

        public async Task LogAsync(string level, string message, string exception = null, string properties = null)
        {
            var log = new Log
            {
                Level = level,
                Message = message,
                Exception = exception,
                Properties = properties,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(log);
        }

        public async Task LogInfoAsync(string message, string properties = null)
        {
            await LogAsync("Information", message, null, properties);
        }

        public async Task LogWarningAsync(string message, string properties = null)
        {
            await LogAsync("Warning", message, null, properties);
        }

        public async Task LogErrorAsync(string message, string exception = null, string properties = null)
        {
            await LogAsync("Error", message, exception, properties);
        }

        private void EnrichLogWithContext(Log log)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                log.Url = context.Request.Path;
                log.HttpMethod = context.Request.Method;
                log.IpAddress = context.Connection.RemoteIpAddress?.ToString();
                log.UserAgent = context.Request.Headers["User-Agent"].ToString();
                
                if (context.User.Identity.IsAuthenticated)
                {
                    var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdString, out var userId))
                    {
                        log.UserId = userId;
                    }
                    log.UserName = context.User.Identity.Name;
                }
            }

            log.MachineName = Environment.MachineName;
            log.Application = "ModularHost";
            
            if (log.CreatedAt == default)
            {
                log.CreatedAt = DateTime.UtcNow;
            }
        }

        public async Task LogAsync(string entityName, string action, string details)
        {
            var log = new Log
            {
                Level = "Information",
                Message = $"Entity: {entityName}, Action: {action}",
                Properties = details,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(log);
        }

        public async Task<List<AuditLogEntry>> GetLogsAsync(int page, int pageSize)
        {
            // Return empty list for now - implement based on your Log entity structure
            return new List<AuditLogEntry>();
        }

        public async Task<int> GetTotalCountAsync()
        {
            // Return 0 for now - implement based on your Log entity structure
            return 0;
        }

        public Task ClearOldLogsAsync(int daysToKeep)
        {
            // Return completed task for now - implement based on your Log entity structure
            return Task.CompletedTask;
        }

        public async Task<LoginStatistics> GetLoginStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // Return empty statistics for now - implement based on your Log entity structure
            return new LoginStatistics
            {
                TotalLogins = 0,
                UniqueUsers = 0,
                FailedAttempts = 0
            };
        }

        public async Task<List<UserLoginCount>> GetUserLoginCountsAsync(DateTime startDate, DateTime endDate, int topCount)
        {
            // Return empty list for now - implement based on your Log entity structure
            return new List<UserLoginCount>();
        }
    }
}