using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nexora.Core.Common;

namespace Nexora.Web.ViewComponents;

public class AppBrandingViewComponent : ViewComponent
{
    private readonly AppSettings _appSettings;

    public AppBrandingViewComponent(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public IViewComponentResult Invoke(string type = "full")
    {
        var model = new AppBrandingViewModel
        {
            Settings = _appSettings,
            Type = type
        };

        return View(model);
    }
}

public class AppBrandingViewModel
{
    public AppSettings Settings { get; set; } = new();
    public string Type { get; set; } = "full"; // full, logo-only, text-only
}