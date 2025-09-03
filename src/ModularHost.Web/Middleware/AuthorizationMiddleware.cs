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
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;

            // Allow public paths without authorization
            var publicPaths = new[]
            {
                "/account/login",
                "/account/logout",
                "/account/register",
                "/account/forgotpassword",
                "/account/resetpassword",
                "/account/confirmaccount",
                "/account/accessdenied",
                "/css/",
                "/js/",
                "/lib/",
                "/images/",
                "/favicon",
                "/error/",
                "/",
                "/home/"
            };

            // Check if current path is public
            if (publicPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
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