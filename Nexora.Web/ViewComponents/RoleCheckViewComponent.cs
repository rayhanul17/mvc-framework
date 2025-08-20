using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Services;

namespace Nexora.Web.ViewComponents;

public class RoleCheckViewComponent : ViewComponent
{
    private readonly IRoleAuthorizationService _roleAuthService;

    public RoleCheckViewComponent(IRoleAuthorizationService roleAuthService)
    {
        _roleAuthService = roleAuthService;
    }

    public async Task<bool> InvokeAsync(string roles)
    {
        var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToArray();

        return await _roleAuthService.IsInAnyRoleAsync(HttpContext.User, roleList);
    }
}