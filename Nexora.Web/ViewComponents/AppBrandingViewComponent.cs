using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nexora.Application.Services;
using Nexora.Core.Common;

namespace Nexora.Web.ViewComponents;

public class AppBrandingViewComponent : ViewComponent
{
    private readonly ISiteSettingHelperService _siteSettings;
    private readonly AppSettings _appSettings;

    public AppBrandingViewComponent(ISiteSettingHelperService siteSettings, IOptions<AppSettings> appSettings)
    {
        _siteSettings = siteSettings;
        _appSettings = appSettings.Value;
    }

    public async Task<IViewComponentResult> InvokeAsync(string type = "full")
    {
        var settings = await _siteSettings.GetAllSettingsAsync();
        var model = new AppBrandingViewModel
        {
            Settings = _appSettings,
            SiteSettings = settings,
            Type = type
        };

        return View(model);
    }
}

public class AppBrandingViewModel
{
    public AppSettings Settings { get; set; } = new();
    public SiteSettingsDynamicModel SiteSettings { get; set; } = new();
    public string Type { get; set; } = "full"; // full, logo-only, text-only
}