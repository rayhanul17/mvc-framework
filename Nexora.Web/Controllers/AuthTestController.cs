using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nexora.Web.Controllers;

public class AuthTestController : Controller
{
    [AllowAnonymous]
    public IActionResult Anonymous()
    {
        return Json(new
        {
            success = true,
            message = "This endpoint allows anonymous access",
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            userName = User.Identity?.Name ?? "Anonymous",
            timestamp = DateTime.Now
        });
    }

    [Authorize]
    public IActionResult Authenticated()
    {
        return Json(new
        {
            success = true,
            message = "This endpoint requires authentication",
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            userName = User.Identity?.Name ?? "Unknown",
            roles = User.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                                .Select(c => c.Value).ToList(),
            timestamp = DateTime.Now
        });
    }

    [Authorize]
    public IActionResult AdminOnly()
    {
        return Json(new
        {
            success = true,
            message = "This endpoint requires Administrator or SuperAdmin role",
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            userName = User.Identity?.Name ?? "Unknown",
            roles = User.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                                .Select(c => c.Value).ToList(),
            timestamp = DateTime.Now
        });
    }

    [Authorize]
    public IActionResult SuperAdminOnly()
    {
        return Json(new
        {
            success = true,
            message = "This endpoint requires SuperAdmin role only",
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            userName = User.Identity?.Name ?? "Unknown",
            roles = User.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                                .Select(c => c.Value).ToList(),
            timestamp = DateTime.Now
        });
    }

    [AllowAnonymous]
    public IActionResult TestPage()
    {
        return View();
    }
}