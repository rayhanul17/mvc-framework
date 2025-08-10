using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Services;

namespace Nexora.Web.ViewComponents;

public class ThemeFooterViewComponent : ViewComponent
{
    private readonly ISiteSettingHelperService _siteSettings;

    public ThemeFooterViewComponent(ISiteSettingHelperService siteSettings)
    {
        _siteSettings = siteSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var settings = await _siteSettings.GetAllSettingsAsync();
        return View(new ThemeFooterViewModel
        {
            FooterText = settings.FooterText,
            CopyrightText = settings.CopyrightText,
            ShowSocialLinks = settings.ShowSocialLinks,
            FacebookUrl = settings.FacebookUrl,
            TwitterUrl = settings.TwitterUrl,
            LinkedInUrl = settings.LinkedInUrl,
            InstagramUrl = settings.InstagramUrl,
            YouTubeUrl = settings.YouTubeUrl,
            GitHubUrl = settings.GitHubUrl,
            OrganizationName = settings.OrganizationName,
            ContactEmail = settings.ContactEmail,
            ContactPhone = settings.ContactPhone,
            Address = settings.Address,
            ShowFooterMenu = settings.ShowFooterMenu
        });
    }
}

public class ThemeFooterViewModel
{
    public string FooterText { get; set; } = string.Empty;
    public string CopyrightText { get; set; } = string.Empty;
    public bool ShowSocialLinks { get; set; }
    public string FacebookUrl { get; set; } = string.Empty;
    public string TwitterUrl { get; set; } = string.Empty;
    public string LinkedInUrl { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool ShowFooterMenu { get; set; }
}