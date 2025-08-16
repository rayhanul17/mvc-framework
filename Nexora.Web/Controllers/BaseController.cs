using Microsoft.AspNetCore.Mvc;
using Nexora.Core.Common;
using Nexora.Core.Enums;
using Serilog;

namespace Nexora.Web.Controllers;

public abstract class BaseController : Controller
{
    private readonly Serilog.ILogger _logger;
    
    protected BaseController()
    {
        _logger = Log.ForContext(GetType());
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
        {
            _logger.Warning("Operation failed: {ErrorMessage}", result.ErrorMessage);
            
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
            
            return BadRequest(ModelState);
        }

        return Ok(result.Data);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (!result.IsSuccess)
        {
            _logger.Warning("Operation failed: {ErrorMessage}", result.ErrorMessage);
            
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
            
            return BadRequest(ModelState);
        }

        return Ok();
    }

    protected void AddErrorsToModelState(Result result)
    {
        if (!result.IsSuccess)
        {
            _logger.Warning("Adding errors to ModelState: {ErrorMessage}", result.ErrorMessage);
            
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
        }
    }

    protected void AddErrorsToModelState<T>(Result<T> result)
    {
        if (!result.IsSuccess)
        {
            _logger.Warning("Adding errors to ModelState: {ErrorMessage}", result.ErrorMessage);
            
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
        }
    }

    // Simplified message methods that use ShowMessage internally
    protected void SetSuccessMessage(string message, bool showAfterRedirect = true)
    {
        ShowMessage(message, MessageType.Success, showAfterRedirect: showAfterRedirect);
    }

    protected void SetErrorMessage(string message, bool showAfterRedirect = true)
    {
        ShowMessage(message, MessageType.Error, showAfterRedirect: showAfterRedirect);
    }

    protected void SetWarningMessage(string message, bool showAfterRedirect = true)
    {
        ShowMessage(message, MessageType.Warning, showAfterRedirect: showAfterRedirect);
    }

    protected void SetInfoMessage(string message, bool showAfterRedirect = true)
    {
        ShowMessage(message, MessageType.Info, showAfterRedirect: showAfterRedirect);
    }

    // Main message method with all options
    public void ShowMessage(string message, MessageType messageType, bool appendMessage = false, 
        bool showAfterRedirect = false, int durationSecond = 5, bool showCloseButton = true)
    {
        string messageKey = messageType switch
        {
            MessageType.Success => "SuccessMessage",
            MessageType.Info => "InfoMessage",
            MessageType.Warning => "WarningMessage",
            MessageType.Error => "ErrorMessage",
            _ => "ErrorMessage"
        };

        if (showAfterRedirect)
        {
            // For redirects, only use TempData
            TempData["MessageDuration"] = durationSecond;
            TempData["MessageShowCloseButton"] = showCloseButton;
            
            if (appendMessage)
            {
                TempData[messageKey] = (TempData[messageKey] as string ?? "") + message;
            }
            else
            {
                TempData[messageKey] = message;
            }
        }
        else
        {
            // For same page display, only use ViewBag
            ViewBag.MessageDuration = durationSecond;
            ViewBag.MessageShowCloseButton = showCloseButton;
            
            if (appendMessage)
            {
                ViewBag[messageKey] = (ViewBag[messageKey] as string ?? "") + message;
            }
            else
            {
                ViewBag[messageKey] = message;
            }
        }

        // Log the message based on type
        switch (messageType)
        {
            case MessageType.Success:
                _logger.Information("UI Message - Success: {Message}", message);
                break;
            case MessageType.Info:
                _logger.Information("UI Message - Info: {Message}", message);
                break;
            case MessageType.Warning:
                _logger.Warning("UI Message - Warning: {Message}", message);
                break;
            case MessageType.Error:
                _logger.Error("UI Message - Error: {Message}", message);
                break;
        }
    }

    // Log methods for different levels
    protected void LogInformation(string message, params object[] args)
    {
        _logger.Information(message, args);
    }

    protected void LogWarning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }

    protected void LogError(string message, params object[] args)
    {
        _logger.Error(message, args);
    }

    protected void LogError(Exception ex, string message, params object[] args)
    {
        _logger.Error(ex, message, args);
    }

    protected void LogDebug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }

    // Helper methods
    protected string GetCurrentUserId()
    {
        var userId = User.Identity?.IsAuthenticated == true 
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty
            : string.Empty;
            
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.Debug("Current User ID: {UserId}", userId);
        }
        
        return userId;
    }

    protected string GetCurrentUserName()
    {
        var userName = User.Identity?.Name ?? string.Empty;
        
        if (!string.IsNullOrEmpty(userName))
        {
            _logger.Debug("Current User Name: {UserName}", userName);
        }
        
        return userName;
    }
    
    protected string GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        _logger.Debug("Client IP Address: {IpAddress}", ipAddress);
        return ipAddress;
    }
    
    protected string GetUserAgent()
    {
        var userAgent = Request.Headers["User-Agent"].ToString();
        _logger.Debug("User Agent: {UserAgent}", userAgent);
        return userAgent;
    }
}