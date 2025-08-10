using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using DynamicRoleMenuSystem.Infrastructure.Data;
using System.Security.Claims;

namespace DynamicRoleMenuSystem.Web.Attributes;

/// <summary>
/// Custom authorization attribute that allows SuperAdmin users to bypass all authorization checks
/// </summary>
public class SuperAdminOrAuthorizeAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    private readonly string? _requiredRoles;

    public SuperAdminOrAuthorizeAttribute(string? roles = null)
    {
        _requiredRoles = roles;
        if (!string.IsNullOrEmpty(roles))
        {
            Roles = roles;
        }
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        // Check if user is authenticated
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get database context
        var dbContext = context.HttpContext.RequestServices.GetService<ApplicationDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Check if user is SuperAdmin
        var appUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (appUser != null && appUser.IsSuperAdmin)
        {
            // SuperAdmin bypasses all checks
            return;
        }

        // Check if user is in SuperAdmin role
        var isSuperAdminRole = await dbContext.UserRoles
            .Join(dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .AnyAsync(x => x.UserId == userId && x.Name == "SuperAdmin");
        
        if (isSuperAdminRole)
        {
            // SuperAdmin role bypasses all checks
            return;
        }

        // If specific roles are required, check them
        if (!string.IsNullOrEmpty(_requiredRoles))
        {
            var requiredRolesList = _requiredRoles.Split(',').Select(r => r.Trim()).ToList();
            var userRoles = await dbContext.UserRoles
                .Join(dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .Where(x => x.UserId == userId)
                .Select(x => x.Name)
                .ToListAsync();

            if (!requiredRolesList.Any(role => userRoles.Contains(role)))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}