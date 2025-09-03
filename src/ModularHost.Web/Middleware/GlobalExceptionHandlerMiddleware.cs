using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MRCMS.Core.Infrastructure.ErrorHandling;
using MRCMS.Services.Interfaces;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MRCMS.Middleware
{
    /// <summary>
    /// Optimized global exception handler middleware with high-performance error handling
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly JsonSerializerOptions _jsonOptions;
        
        // Cache for JSON responses to avoid repeated serialization
        private static readonly ConcurrentDictionary<string, byte[]> ResponseCache = new();
        private const int MaxCacheSize = 100;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
            
            // Pre-configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = environment.IsDevelopment()
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                
                // Don't handle 404 here - let UseStatusCodePagesWithReExecute handle it
                // Only handle exceptions
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception asynchronously without blocking
            _ = Task.Run(() => LogException(context, exception));

            // Get error details using optimized handler
            var errorDetails = ExceptionHandler.GetErrorDetails(exception, _environment.IsDevelopment());
            
            // Set response status code
            context.Response.StatusCode = errorDetails.StatusCode;

            // Fast path for API requests
            if (IsApiRequest(context))
            {
                await HandleApiExceptionAsync(context, errorDetails, exception);
            }
            else
            {
                await HandleWebExceptionAsync(context, errorDetails, exception);
            }
        }


        private async Task HandleApiExceptionAsync(HttpContext context, ErrorDetails errorDetails, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = new
                {
                    code = errorDetails.StatusCode,
                    message = errorDetails.Message,
                    details = _environment.IsDevelopment() ? errorDetails.Details : null,
                    stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
                    timestamp = DateTime.UtcNow
                }
            };

            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(response, _jsonOptions);
            await context.Response.Body.WriteAsync(jsonBytes);
        }

        private async Task HandleWebExceptionAsync(HttpContext context, ErrorDetails errorDetails, Exception exception)
        {
            // Store exception details in Items (faster than TempData)
            context.Items["ExceptionMessage"] = errorDetails.Message;
            context.Items["ExceptionDetails"] = errorDetails.Details;
            context.Items["ExceptionStatusCode"] = errorDetails.StatusCode;
            
            if (_environment.IsDevelopment())
            {
                context.Items["ExceptionStackTrace"] = exception.StackTrace;
            }

            // Clear the response if not started
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = errorDetails.StatusCode;
                
                // For 401, redirect to login
                if (errorDetails.StatusCode == 401)
                {
                    var returnUrl = Uri.EscapeDataString(context.Request.Path);
                    context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                }
                else
                {
                    // For other errors, re-execute with error path
                    var errorPath = $"/Error/{errorDetails.StatusCode}";
                    context.Request.Path = errorPath;
                    await _next(context);
                }
            }
        }

        private void LogException(HttpContext context, Exception exception)
        {
            try
            {
                // Enhanced logging with more context
                var logContext = new
                {
                    Timestamp = DateTime.UtcNow,
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId,
                    ThreadId = Environment.CurrentManagedThreadId,
                    RequestId = context.TraceIdentifier,
                    RequestPath = context.Request.Path.Value,
                    RequestMethod = context.Request.Method,
                    QueryString = context.Request.QueryString.Value,
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    User = context.User?.Identity?.Name ?? "Anonymous",
                    ExceptionType = exception.GetType().FullName,
                    InnerException = exception.InnerException?.GetType().FullName
                };

                _logger.LogError(exception, 
                    "Unhandled exception occurred | Context: {@LogContext}", 
                    logContext);

                // Check for specific exception types that might cause crashes
                if (exception is StackOverflowException || 
                    exception is OutOfMemoryException ||
                    exception is AccessViolationException)
                {
                    _logger.LogCritical(exception,
                        "CRITICAL: Application crash detected - {ExceptionType} | Path: {Path}",
                        exception.GetType().Name,
                        context.Request.Path);
                }

                // Try to log to audit service if available
                var loggerService = context.RequestServices.GetService(typeof(ILoggerService)) as ILoggerService;
                loggerService?.LogError($"Unhandled exception: {exception.Message} | Path: {context.Request.Path}", exception);
            }
            catch (Exception logEx)
            {
                // Last resort logging to console
                Console.WriteLine($"[CRITICAL] Failed to log exception: {logEx.Message}");
                Console.WriteLine($"[CRITICAL] Original exception: {exception.Message}");
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            // Fast path checks
            if (context.Request.Path.StartsWithSegments("/api"))
                return true;

            var acceptHeader = context.Request.Headers["Accept"].ToString();
            if (!string.IsNullOrEmpty(acceptHeader) && acceptHeader.Contains("application/json"))
                return true;

            var contentType = context.Request.Headers["Content-Type"].ToString();
            return !string.IsNullOrEmpty(contentType) && contentType.Contains("application/json");
        }
    }

    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}