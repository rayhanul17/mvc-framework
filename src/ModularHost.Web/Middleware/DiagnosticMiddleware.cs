using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MRCMS.Middleware
{
    /// <summary>
    /// Diagnostic middleware to track performance and detect potential issues
    /// </summary>
    public class DiagnosticMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DiagnosticMiddleware> _logger;
        private static long _requestCount = 0;
        private static long _errorCount = 0;
        private static readonly object _lock = new object();

        public DiagnosticMiddleware(
            RequestDelegate next,
            ILogger<DiagnosticMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Interlocked.Increment(ref _requestCount);
            var stopwatch = Stopwatch.StartNew();
            var path = context.Request.Path;
            
            // Add diagnostic headers
            context.Response.Headers.Add("X-Request-Id", requestId.ToString());
            context.Response.Headers.Add("X-Process-Id", Environment.ProcessId.ToString());
            
            try
            {
                // Log slow request warning if it takes too long
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                cts.Token.Register(() =>
                {
                    if (!context.Response.HasStarted)
                    {
                        _logger.LogWarning(
                            "Slow request detected | Path: {Path} | Duration: {Duration}ms | RequestId: {RequestId}",
                            path, stopwatch.ElapsedMilliseconds, requestId);
                    }
                });

                await _next(context);
                
                stopwatch.Stop();
                
                // Log performance metrics for slow requests
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "Request completed slowly | Path: {Path} | Duration: {Duration}ms | Status: {StatusCode}",
                        path, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
                }
                
                // Check for potential memory issues
                var memoryUsed = GC.GetTotalMemory(false) / (1024 * 1024); // Convert to MB
                if (memoryUsed > 500) // Warning if over 500MB
                {
                    _logger.LogWarning(
                        "High memory usage detected | Memory: {Memory}MB | Path: {Path}",
                        memoryUsed, path);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _errorCount);
                stopwatch.Stop();
                
                _logger.LogError(ex,
                    "Request failed | Path: {Path} | Duration: {Duration}ms | RequestId: {RequestId} | ErrorCount: {ErrorCount}",
                    path, stopwatch.ElapsedMilliseconds, requestId, _errorCount);
                    
                // Check for cascade failures
                if (_errorCount > 10)
                {
                    _logger.LogCritical(
                        "High error rate detected | ErrorCount: {ErrorCount} | LastPath: {Path}",
                        _errorCount, path);
                }
                
                throw;
            }
        }
    }

    public static class DiagnosticMiddlewareExtensions
    {
        public static IApplicationBuilder UseDiagnostics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DiagnosticMiddleware>();
        }
    }
}