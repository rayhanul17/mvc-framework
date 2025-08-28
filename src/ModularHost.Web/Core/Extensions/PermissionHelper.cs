using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ModularHost.Web.Core.Extensions
{
    public interface IPermissionHelper
    {
        bool HasPermission(string permission);
        bool IsInRole(string role);
        bool IsAuthenticated();
        bool IsAdmin();
        bool IsSuperAdmin();
        bool CanEdit(Guid? resourceOwnerId);
        bool CanDelete(Guid? resourceOwnerId);
        Guid GetCurrentUserId();
        string GetCurrentUserName();
    }

    public class PermissionHelper : IPermissionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClaimsPrincipal? _user;

        public PermissionHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _user = _httpContextAccessor.HttpContext?.User;
        }

        public bool HasPermission(string permission)
        {
            if (_user == null) return false;
            return _user.HasClaim("Permission", permission);
        }

        public bool IsInRole(string role)
        {
            if (_user == null) return false;
            return _user.IsInRole(role);
        }

        public bool IsAuthenticated()
        {
            return _user?.Identity?.IsAuthenticated ?? false;
        }

        public bool IsAdmin()
        {
            return IsInRole("Admin") || IsInRole("SuperAdmin");
        }

        public bool IsSuperAdmin()
        {
            return IsInRole("SuperAdmin");
        }

        public bool CanEdit(Guid? resourceOwnerId)
        {
            if (!IsAuthenticated()) return false;
            if (IsAdmin()) return true;
            if (resourceOwnerId == null) return false;
            return resourceOwnerId == GetCurrentUserId();
        }

        public bool CanDelete(Guid? resourceOwnerId)
        {
            if (!IsAuthenticated()) return false;
            if (IsSuperAdmin()) return true;
            if (IsAdmin()) return true;
            if (resourceOwnerId == null) return false;
            return resourceOwnerId == GetCurrentUserId();
        }

        public Guid GetCurrentUserId()
        {
            var userIdClaim = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        public string GetCurrentUserName()
        {
            return _user?.Identity?.Name ?? "Anonymous";
        }
    }
}