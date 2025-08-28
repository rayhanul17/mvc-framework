using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nexora.Application.Interfaces;
using System.Security.Claims;

namespace Nexora.Web.Middleware;

public class UrlAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UrlAuthorizationMiddleware> _logger;
    
    public UrlAuthorizationMiddleware(RequestDelegate next, ILogger<UrlAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, IUrlAuthorizationService authorizationService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }
        
        var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor == null)
        {
            await _next(context);
            return;
        }
        
        // Get authorization attributes
        var allowAnonymous = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
        var authorize = endpoint.Metadata.GetMetadata<AuthorizeAttribute>() != null;
        
        // Check controller attributes if action doesn't have any
        if (!allowAnonymous && !authorize)
        {
            var controllerType = actionDescriptor.ControllerTypeInfo;
            allowAnonymous = controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
            authorize = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        }
        
        // Default to AllowAnonymous if no attributes
        if (!allowAnonymous && !authorize)
        {
            allowAnonymous = true;
        }
        
        // Allow anonymous access
        if (allowAnonymous)
        {
            await _next(context);
            return;
        }
        
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        
        // Get user ID
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        
        // Check if user is SuperAdmin (bypass all checks)
        var isSuperAdmin = context.User.Claims.Any(c => c.Type == "IsSuperAdmin" && c.Value == "true");
        if (isSuperAdmin)
        {
            await _next(context);
            return;
        }
        
        // Build URL for permission check
        var area = actionDescriptor.RouteValues["area"];
        var controller = actionDescriptor.ControllerName;
        var action = actionDescriptor.ActionName;
        
        // Check URL-based permissions
        var hasAccess = await authorizationService.HasAccessToActionAsync(userId, area, controller, action);
        
        if (!hasAccess)
        {
            // Also check direct URL permission
            var path = context.Request.Path.Value?.ToLower() ?? "/";
            hasAccess = await authorizationService.HasAccessToUrlAsync(userId, path);
        }
        
        if (!hasAccess)
        {
            _logger.LogWarning($"User {userId} denied access to {area}/{controller}/{action}");
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden");
            return;
        }
        
        await _next(context);
    }
}