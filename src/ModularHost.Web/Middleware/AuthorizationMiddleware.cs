using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using ModularHost.Web.Core.Enums;
using ModularHost.Web.Attributes;
using ModularHost.Web.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModularHost.Web.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IPermissionService permissionService)
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
                await context.Response.WriteAsync("Unauthorized");
            }
            else
            {
                context.Response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path));
            }
        }

        private async Task HandleForbidden(HttpContext context)
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
            }
            else
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("You don't have permission to access this resource.");
            }
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}