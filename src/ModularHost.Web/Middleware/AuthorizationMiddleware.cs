using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IPermissionService permissionService, AppDbContext dbContext)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            // Allow Account controller actions and static files without authorization
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.StartsWith("/account/") || 
                path.StartsWith("/css/") || 
                path.StartsWith("/js/") || 
                path.StartsWith("/lib/") || 
                path.StartsWith("/favicon") ||
                path == "/" ||  // Allow home page for now
                path.StartsWith("/home/"))
            {
                await _next(context);
                return;
            }

            var authorizeAttribute = endpoint.Metadata.GetMetadata<UrlAuthorizeAttribute>();
            if (authorizeAttribute == null)
            {
                // Default to require authorization if not specified
                authorizeAttribute = new UrlAuthorizeAttribute { Policy = AuthorizationPolicyType.Authorize };
            }

            var method = context.Request.Method;

            switch (authorizeAttribute.Policy)
            {
                case AuthorizationPolicyType.Anonymous:
                    await _next(context);
                    return;

                case AuthorizationPolicyType.Authenticate:
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        await HandleUnauthorized(context);
                        return;
                    }
                    break;

                case AuthorizationPolicyType.Authorize:
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        await HandleUnauthorized(context);
                        return;
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

                    var url = authorizeAttribute.Url ?? path;
                    var httpMethod = authorizeAttribute.HttpMethod ?? method;
                    
                    if (!await permissionService.IsUrlAllowedAsync(context.User, url, httpMethod))
                    {
                        await HandleForbidden(context);
                        return;
                    }
                    break;
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