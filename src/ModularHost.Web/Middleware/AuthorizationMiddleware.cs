using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MRCMS.Core.Enums;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Attributes;
using MRCMS.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MRCMS.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthorizationMiddleware> _logger;

        public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IPermissionService permissionService, AppDbContext dbContext)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;

            // Always allow static resources
            var staticPaths = new[]
            {
                "/css/",
                "/js/",
                "/lib/",
                "/images/",
                "/favicon"
            };

            if (staticPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Check for URL-specific permissions in database
            _logger.LogDebug("Checking permissions for path: {Path}, method: {Method}", path, method);
            
            RolePermission? urlPermission = null;
            try
            {
                urlPermission = await dbContext.RolePermissions
                    .AsNoTracking()
                    .Where(rp => !rp.IsDeleted && rp.IsActive)
                    .FirstOrDefaultAsync(rp => 
                        (rp.Url == path || rp.Url == path + "/*" || 
                         (rp.Url.EndsWith("/*") && path.StartsWith(rp.Url.Replace("/*", "")))) &&
                        (rp.HttpMethod == "*" || rp.HttpMethod == method));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions in database for path: {Path}", path);
                // On database error, default to requiring authentication
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    await HandleUnauthorized(context);
                    return;
                }
            }

            // If no permission record exists for this URL, check default public paths
            if (urlPermission == null)
            {
                var defaultPublicPaths = new[]
                {
                    "/",
                    "/home",
                    "/account/login",
                    "/account/logout",
                    "/account/register",
                    "/account/forgotpassword",
                    "/account/resetpassword",
                    "/account/confirmaccount",
                    "/account/accessdenied",
                    "/error/"
                };

                if (defaultPublicPaths.Any(p => path.StartsWith(p)))
                {
                    await _next(context);
                    return;
                }

                // No permission defined and not a public path - require authentication by default
                _logger.LogDebug("No permission defined for path: {Path}, requiring authentication", path);
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("Unauthorized access attempt to: {Path} by anonymous user", path);
                    await HandleUnauthorized(context);
                    return;
                }
            }
            else
            {
                // Permission record exists - check AccessType
                _logger.LogDebug("Permission found for path: {Path}, AccessType: {AccessType}", path, urlPermission.AccessType);
                switch (urlPermission.AccessType)
                {
                    case AccessType.Anonymous:
                        // Anyone can access
                        _logger.LogDebug("Anonymous access allowed for path: {Path}", path);
                        await _next(context);
                        return;

                    case AccessType.Authenticated:
                        // Must be logged in
                        if (!context.User.Identity?.IsAuthenticated ?? true)
                        {
                            _logger.LogWarning("Unauthorized access attempt to authenticated-only path: {Path}", path);
                            await HandleUnauthorized(context);
                            return;
                        }
                        _logger.LogDebug("Authenticated access granted for path: {Path}, user: {User}", path, context.User.Identity?.Name ?? "Unknown");
                        break;

                    case AccessType.Authorized:
                        // Must be logged in and have specific role permission
                        if (!context.User.Identity?.IsAuthenticated ?? true)
                        {
                            _logger.LogWarning("Unauthorized access attempt to role-protected path: {Path}", path);
                            await HandleUnauthorized(context);
                            return;
                        }
                        _logger.LogDebug("Checking role-based authorization for path: {Path}, user: {User}", path, context.User.Identity?.Name ?? "Unknown");
                        // Continue with role-based authorization check below
                        break;
                }
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Check if user is SuperAdmin from database - they bypass all permission checks
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user != null && user.IsSuperAdmin)
                {
                    await _next(context);
                    return;
                }
            }

            // Check if user has permission for this URL through their roles
            if (!await permissionService.IsUrlAllowedAsync(context.User, path, method))
            {
                await HandleForbidden(context);
                return;
            }

            await _next(context);
        }

        private async Task HandleUnauthorized(HttpContext context)
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Unauthorized\",\"message\":\"Authentication required\"}");
            }
            else
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
            }
        }

        private async Task HandleForbidden(HttpContext context)
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Forbidden\",\"message\":\"You don't have permission to access this resource\"}");
            }
            else
            {
                context.Response.Redirect("/Error/Forbidden");
            }
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}