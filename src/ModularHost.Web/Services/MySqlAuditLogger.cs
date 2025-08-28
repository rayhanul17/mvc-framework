using Microsoft.AspNetCore.Http;
using ModularHost.Web.Core.Models.Entities;
using ModularHost.Web.Core.Infrastructure;
using ModularHost.Web.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModularHost.Web.Services
{
    public class MySqlAuditLogger : IAuditLogger
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MySqlAuditLogger(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(Log log)
        {
            EnrichLogWithContext(log);
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
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
    }
}