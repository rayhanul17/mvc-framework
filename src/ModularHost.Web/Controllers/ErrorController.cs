using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRCMS.Models.ViewModels;
using System.Diagnostics;

namespace MRCMS.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorController(ILogger<ErrorController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        [Route("Error")]
        [Route("Error/Index")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            var statusCode = HttpContext.Items["ExceptionStatusCode"] as int? ?? 500;
            var message = HttpContext.Items["ExceptionMessage"] as string ?? "An error occurred while processing your request.";
            var details = HttpContext.Items["ExceptionDetails"] as string;
            var stackTrace = HttpContext.Items["ExceptionStackTrace"] as string;

            // Also check for IExceptionHandlerFeature
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                _logger.LogError(exceptionFeature.Error, "Error caught by exception handler");
                if (_environment.IsDevelopment() && string.IsNullOrEmpty(stackTrace))
                {
                    stackTrace = exceptionFeature.Error.StackTrace;
                }
            }

            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                Message = message,
                Details = details,
                StackTrace = _environment.IsDevelopment() ? stackTrace : null,
                ShowDetails = _environment.IsDevelopment()
            };

            Response.StatusCode = statusCode;
            return View(model);
        }

        [Route("Error/NotFound")]
        [Route("404")]
        public new IActionResult NotFound()
        {
            Response.StatusCode = 404;
            
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = 404,
                Message = "Page Not Found",
                Details = $"The page you are looking for doesn't exist or has been moved.",
                RequestedPath = HttpContext.Request.Path
            };

            return View("NotFound", model);
        }

        [Route("Error/Forbidden")]
        [Route("403")]
        public IActionResult Forbidden()
        {
            Response.StatusCode = 403;
            
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = 403,
                Message = "Access Denied",
                Details = "You don't have permission to access this resource.",
                RequestedPath = HttpContext.Request.Path
            };

            return View("Forbidden", model);
        }

        [Route("Error/Unauthorized")]
        [Route("401")]
        public IActionResult Unauthorized(string? returnUrl = null)
        {
            Response.StatusCode = 401;
            
            // If user is not authenticated, redirect to login
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? HttpContext.Request.Path });
            }

            // User is authenticated but not authorized
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = 401,
                Message = "Unauthorized",
                Details = "You need different permissions to access this resource.",
                RequestedPath = HttpContext.Request.Path
            };

            return View("Unauthorized", model);
        }

        [Route("Error/{statusCode:int}")]
        public new IActionResult StatusCode(int statusCode)
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                Message = GetStatusCodeMessage(statusCode),
                Details = GetStatusCodeDetails(statusCode),
                RequestedPath = HttpContext.Request.Headers["X-Original-Path"].FirstOrDefault() ?? HttpContext.Request.Path
            };

            Response.StatusCode = statusCode;
            
            // Use specific views for common status codes
            return statusCode switch
            {
                404 => View("NotFound", model),
                403 => View("Forbidden", model),
                401 => View("Unauthorized", model),
                _ => View("Index", model)
            };
        }

        // Pre-compiled dictionaries for O(1) lookup performance
        private static readonly Dictionary<int, string> StatusCodeMessages = new()
        {
            [400] = "Bad Request",
            [401] = "Unauthorized",
            [403] = "Forbidden",
            [404] = "Not Found",
            [405] = "Method Not Allowed",
            [408] = "Request Timeout",
            [409] = "Conflict",
            [410] = "Gone",
            [429] = "Too Many Requests",
            [500] = "Internal Server Error",
            [501] = "Not Implemented",
            [502] = "Bad Gateway",
            [503] = "Service Unavailable",
            [504] = "Gateway Timeout"
        };

        private static readonly Dictionary<int, string> StatusCodeDetails = new()
        {
            [400] = "The request could not be understood by the server.",
            [401] = "Authentication is required to access this resource.",
            [403] = "You don't have permission to access this resource.",
            [404] = "The requested resource could not be found.",
            [405] = "The method specified in the request is not allowed.",
            [408] = "The server timed out waiting for the request.",
            [409] = "The request could not be completed due to a conflict.",
            [410] = "The requested resource is no longer available.",
            [429] = "Too many requests have been made. Please try again later.",
            [500] = "The server encountered an unexpected condition.",
            [501] = "The server does not support the functionality required.",
            [502] = "The server received an invalid response from the upstream server.",
            [503] = "The server is currently unavailable.",
            [504] = "The server did not receive a timely response from the upstream server."
        };

        private string GetStatusCodeMessage(int statusCode)
        {
            return StatusCodeMessages.TryGetValue(statusCode, out var message) 
                ? message 
                : "An Error Occurred";
        }

        private string GetStatusCodeDetails(int statusCode)
        {
            return StatusCodeDetails.TryGetValue(statusCode, out var details) 
                ? details 
                : "An unexpected error occurred while processing your request.";
        }
    }
}