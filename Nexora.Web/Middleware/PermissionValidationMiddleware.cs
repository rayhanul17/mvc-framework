using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nexora.Application.Interfaces;
using Nexora.Web.Attributes;
using System.Reflection;
using System.Security.Claims;

namespace Nexora.Web.Middleware;

/// <summary>
/// Middleware to validate permissions for requests automatically
/// This provides an additional layer of security beyond controller-level authorization
/// </summary>
public class PermissionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionValidationMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PermissionValidationMiddleware(
        RequestDelegate next, 
        ILogger<PermissionValidationMiddleware> logger,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for certain requests
        if (ShouldSkipValidation(context))
        {
            await _next(context);
            return;
        }

        // Extract route information
        var endpoint = context.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        
        if (actionDescriptor == null)
        {
            await _next(context);
            return;
        }

        var area = context.Request.RouteValues["area"]?.ToString() ?? "";
        var controller = actionDescriptor.ControllerName;
        var action = actionDescriptor.ActionName;

        // Check if the action has authorization attributes
        var hasAllowAnonymous = HasAllowAnonymousAttribute(actionDescriptor);
        var hasDynamicPermission = HasDynamicPermissionAttribute(actionDescriptor);
        var hasAuthorize = HasAuthorizeAttribute(actionDescriptor);

        // Skip validation if action allows anonymous access
        if (hasAllowAnonymous)
        {
            await _next(context);
            return;
        }

        // For authenticated users, validate permissions
        if (context.User.Identity?.IsAuthenticated == true)
        {
            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetService<IPermissionService>();
            
            if (permissionService != null)
            {
                try
                {
                    // Check if user is SuperAdmin (bypass all permission checks)
                    if (await permissionService.IsSuperAdminAsync(context.User))
                    {
                        _logger.LogDebug("SuperAdmin access granted for {Area}/{Controller}/{Action}", area, controller, action);
                        await _next(context);
                        return;
                    }

                    // Check specific permission if the action has dynamic permission attribute
                    if (hasDynamicPermission || hasAuthorize)
                    {
                        var hasPermission = await permissionService.HasPermissionAsync(context.User, area, controller, action);
                        
                        if (hasPermission)
                        {
                            _logger.LogDebug("Permission granted for {Area}/{Controller}/{Action} to user {UserId}", 
                                area, controller, action, context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                            await _next(context);
                            return;
                        }
                        else
                        {
                            _logger.LogWarning("Permission denied for {Area}/{Controller}/{Action} to user {UserId}", 
                                area, controller, action, context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                            
                            // Return forbidden response
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("Access Denied: You don't have permission to access this resource.");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating permissions for {Area}/{Controller}/{Action}", area, controller, action);
                    // In case of error, allow the request to continue to avoid blocking the application
                }
            }
        }
        else if (hasAuthorize || hasDynamicPermission)
        {
            // User is not authenticated but action requires authorization
            _logger.LogWarning("Unauthenticated access attempt to {Area}/{Controller}/{Action}", area, controller, action);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication required to access this resource.");
            return;
        }

        await _next(context);
    }

    private bool ShouldSkipValidation(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        // Skip validation for static files
        if (path.Contains("/css/") || path.Contains("/js/") || path.Contains("/images/") || 
            path.Contains("/lib/") || path.Contains("/favicon.") || path.Contains("/robots.txt"))
        {
            return true;
        }

        // Skip validation for API documentation
        if (path.Contains("/swagger") || path.Contains("/api-docs"))
        {
            return true;
        }

        // Skip validation for error pages
        if (path.Contains("/error") || path.Contains("/404") || path.Contains("/500"))
        {
            return true;
        }

        // Skip validation for health checks
        if (path.Contains("/health") || path.Contains("/ping"))
        {
            return true;
        }

        return false;
    }

    private bool HasAllowAnonymousAttribute(ControllerActionDescriptor actionDescriptor)
    {
        // Check action method
        var actionMethod = actionDescriptor.MethodInfo;
        if (actionMethod.GetCustomAttribute<AllowAnonymousAttribute>() != null)
            return true;

        // Check controller
        var controllerType = actionDescriptor.ControllerTypeInfo;
        if (controllerType.GetCustomAttribute<AllowAnonymousAttribute>() != null)
            return true;

        return false;
    }

    private bool HasDynamicPermissionAttribute(ControllerActionDescriptor actionDescriptor)
    {
        // Check action method
        var actionMethod = actionDescriptor.MethodInfo;
        if (actionMethod.GetCustomAttribute<DynamicPermissionAuthorizeAttribute>() != null)
            return true;

        // Check controller
        var controllerType = actionDescriptor.ControllerTypeInfo;
        if (controllerType.GetCustomAttribute<DynamicPermissionAuthorizeAttribute>() != null)
            return true;

        return false;
    }

    private bool HasAuthorizeAttribute(ControllerActionDescriptor actionDescriptor)
    {
        // Check action method
        var actionMethod = actionDescriptor.MethodInfo;
        if (actionMethod.GetCustomAttribute<AuthorizeAttribute>() != null)
            return true;

        // Check controller
        var controllerType = actionDescriptor.ControllerTypeInfo;
        if (controllerType.GetCustomAttribute<AuthorizeAttribute>() != null)
            return true;

        return false;
    }
}

/// <summary>
/// Extension method to register the permission validation middleware
/// </summary>
public static class PermissionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UsePermissionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PermissionValidationMiddleware>();
    }
}