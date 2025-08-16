using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Web.Models;
using System.Diagnostics;

namespace Nexora.Web.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;
    private readonly ISiteSettingService _siteSettingService;

    public ErrorController(ILogger<ErrorController> logger, ISiteSettingService siteSettingService)
    {
        _logger = logger;
        _siteSettingService = siteSettingService;
    }

    [Route("Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(int? statusCode = null)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var originalPath = HttpContext.Items["originalPath"]?.ToString();
        
        // Get site settings
        var siteSettingsResult = await _siteSettingService.GetAllSettingsAsync();
        var siteName = "Nexora Framework";
        if (siteSettingsResult.IsSuccess && siteSettingsResult.Data != null)
        {
            siteName = siteSettingsResult.Data.FirstOrDefault(s => s.Key == "SiteName")?.Value ?? siteName;
        }

        var model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode ?? 500,
            OriginalPath = originalPath,
            SiteName = siteName,
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
            Timestamp = DateTime.UtcNow
        };

        // Log the error details
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature?.Error != null)
        {
            _logger.LogError(exceptionFeature.Error, 
                "Unhandled exception occurred. StatusCode: {StatusCode}, Path: {Path}, RequestId: {RequestId}", 
                statusCode, originalPath, requestId);
                
            model.ErrorMessage = exceptionFeature.Error.Message;
            model.ExceptionType = exceptionFeature.Error.GetType().Name;
        }
        else
        {
            _logger.LogWarning("Error page accessed. StatusCode: {StatusCode}, Path: {Path}, RequestId: {RequestId}", 
                statusCode, originalPath, requestId);
        }

        // Set appropriate response status code
        if (statusCode.HasValue)
        {
            HttpContext.Response.StatusCode = statusCode.Value;
        }

        // Determine which error view to use
        return statusCode switch
        {
            404 => View("NotFound", model),
            403 => View("AccessDenied", model),
            500 => View("ServerError", model),
            _ => View("General", model)
        };
    }

    [Route("Error/NotFound")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> NotFound()
    {
        return await Index(404);
    }

    [Route("Error/AccessDenied")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> AccessDenied()
    {
        return await Index(403);
    }

    [Route("Error/ServerError")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ServerError()
    {
        return await Index(500);
    }
}