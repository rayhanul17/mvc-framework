using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DynamicRoleMenuSystem.Core.Common;

namespace DynamicRoleMenuSystem.Web.ViewComponents;

public class ThemeStylesViewComponent : ViewComponent
{
    private readonly AppSettings _appSettings;

    public ThemeStylesViewComponent(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public IViewComponentResult Invoke()
    {
        return View(_appSettings);
    }
}