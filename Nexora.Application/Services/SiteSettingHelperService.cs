using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;

namespace Nexora.Application.Services;

public interface ISiteSettingHelperService
{
    Task<string> GetSettingValueAsync(string key, string defaultValue = "");
    Task<T> GetSettingValueAsync<T>(string key, T defaultValue = default(T));
    Task<bool> GetBoolAsync(string key, bool defaultValue = false);
    Task<SiteSettingsDynamicModel> GetAllSettingsAsync();
}

public class SiteSettingHelperService : ISiteSettingHelperService
{
    private readonly ISiteSettingService _siteSettingService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SiteSettingHelperService> _logger;
    private const string CACHE_KEY = "SiteSettings:AllForLayout";
    private const int CACHE_EXPIRATION_MINUTES = 15;

    public SiteSettingHelperService(ISiteSettingService siteSettingService, IMemoryCache cache, ILogger<SiteSettingHelperService> logger)
    {
        _siteSettingService = siteSettingService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetSettingValueAsync(string key, string defaultValue = "")
    {
        try
        {
            var result = await _siteSettingService.GetSettingValueAsync(key, defaultValue);
            return result.IsSuccess ? result.Data : defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting value for key {Key}", key);
            return defaultValue;
        }
    }

    public async Task<T> GetSettingValueAsync<T>(string key, T defaultValue = default(T))
    {
        try
        {
            var result = await _siteSettingService.GetSettingValueAsync<T>(key, defaultValue);
            return result.IsSuccess ? result.Data : defaultValue!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting value for key {Key}", key);
            return defaultValue!;
        }
    }

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        return await GetSettingValueAsync(key, defaultValue);
    }

    public async Task<SiteSettingsDynamicModel> GetAllSettingsAsync()
    {
        if (_cache.TryGetValue(CACHE_KEY, out SiteSettingsDynamicModel? cachedModel) && cachedModel != null)
        {
            return cachedModel;
        }

        try
        {
            var model = new SiteSettingsDynamicModel();

            // Branding settings
            model.SiteName = await GetSettingValueAsync("SiteName", "Dynamic Role Menu System");
            model.SiteDescription = await GetSettingValueAsync("SiteDescription", "A comprehensive role-based menu management system");
            model.OrganizationName = await GetSettingValueAsync("OrganizationName", "Your Organization");
            model.ShortName = await GetSettingValueAsync("ShortName", "DRMS");
            model.Slogan = await GetSettingValueAsync("Slogan", "Empowering Your Digital Future");
            model.LogoPath = await GetSettingValueAsync("LogoPath", "/images/logo.png");
            model.SmallLogo = await GetSettingValueAsync("SmallLogo", "/images/logo-small.png");
            model.LargeLogo = await GetSettingValueAsync("LargeLogo", "/images/logo-large.png");
            model.FaviconPath = await GetSettingValueAsync("FaviconPath", "/favicon.ico");
            model.LoadingLogoPath = await GetSettingValueAsync("LoadingLogoPath", "");

            // Theme settings
            model.PrimaryColor = await GetSettingValueAsync("PrimaryColor", "#0d6efd");
            model.SecondaryColor = await GetSettingValueAsync("SecondaryColor", "#6c757d");
            model.SuccessColor = await GetSettingValueAsync("SuccessColor", "#198754");
            model.DangerColor = await GetSettingValueAsync("DangerColor", "#dc3545");
            model.WarningColor = await GetSettingValueAsync("WarningColor", "#ffc107");
            model.DarkMode = await GetBoolAsync("DarkModeDefault", false);

            // Footer settings
            model.FooterCompanyName = await GetSettingValueAsync("OrganizationName", "Dynamic Role Menu System");
            model.FooterCopyrightYear = DateTime.UtcNow.Year.ToString();
            model.FooterShowPoweredBy = await GetBoolAsync("FooterShowPoweredBy", true);
            model.FooterCustomText = await GetSettingValueAsync("FooterCustomText", "");
            model.FooterText = await GetSettingValueAsync("FooterText", "Building innovative solutions for modern businesses.");
            model.FooterHtml = await GetSettingValueAsync("FooterHtml", "");
            model.CopyrightText = await GetSettingValueAsync("CopyrightText", "");
            model.ShowSocialLinks = await GetBoolAsync("ShowSocialLinks", true);
            model.ShowFooterMenu = await GetBoolAsync("ShowFooterMenu", true);
            model.StickyFooter = await GetBoolAsync("StickyFooter", true);

            // Contact settings
            model.ContactEmail = await GetSettingValueAsync("ContactEmail", "admin@example.com");
            model.ContactPhone = await GetSettingValueAsync("PhoneNumber", "");
            model.ContactAddress = await GetSettingValueAsync("Address", "");
            model.Address = await GetSettingValueAsync("Address", "");
            
            // Social Media settings
            model.FacebookUrl = await GetSettingValueAsync("FacebookUrl", "");
            model.TwitterUrl = await GetSettingValueAsync("TwitterUrl", "");
            model.LinkedInUrl = await GetSettingValueAsync("LinkedInUrl", "");
            model.InstagramUrl = await GetSettingValueAsync("InstagramUrl", "");
            model.YouTubeUrl = await GetSettingValueAsync("YouTubeUrl", "");
            model.GitHubUrl = await GetSettingValueAsync("GitHubUrl", "");

            // SEO settings
            model.MetaTitle = await GetSettingValueAsync("SEO.MetaTitle", "Dynamic Role Menu System");
            model.MetaDescription = await GetSettingValueAsync("SEO.MetaDescription", "A comprehensive role-based menu management system built with ASP.NET Core MVC framework");
            model.MetaKeywords = await GetSettingValueAsync("SEO.MetaKeywords", "role management, menu system, asp.net core, mvc, authorization");

            // System settings
            model.Version = await GetSettingValueAsync("System.Version", "1.0.0");
            model.MaintenanceMode = await GetBoolAsync("System.MaintenanceMode", false);
            model.AllowRegistration = await GetBoolAsync("System.AllowRegistration", false);
            model.DefaultUserRole = await GetSettingValueAsync("System.DefaultUserRole", "User");

            _cache.Set(CACHE_KEY, model, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all site settings");
            return new SiteSettingsDynamicModel(); // Return default model
        }
    }
}

public class SiteSettingsDynamicModel
{
    // Branding
    public string SiteName { get; set; } = "Dynamic Role Menu System";
    public string SiteDescription { get; set; } = "A comprehensive role-based menu management system";
    public string OrganizationName { get; set; } = "Your Organization";
    public string ShortName { get; set; } = "DRMS";
    public string Slogan { get; set; } = "Empowering Your Digital Future";
    public string LogoPath { get; set; } = "/images/logo.png";
    public string SmallLogo { get; set; } = "/images/logo-small.png";
    public string LargeLogo { get; set; } = "/images/logo-large.png";
    public string FaviconPath { get; set; } = "/favicon.ico";
    public string LoadingLogoPath { get; set; } = "";

    // Theme
    public string PrimaryColor { get; set; } = "#0d6efd";
    public string SecondaryColor { get; set; } = "#6c757d";
    public string SuccessColor { get; set; } = "#198754";
    public string DangerColor { get; set; } = "#dc3545";
    public string WarningColor { get; set; } = "#ffc107";
    public bool DarkMode { get; set; } = false;

    // Footer
    public string FooterCompanyName { get; set; } = "Dynamic Role Menu System";
    public string FooterCopyrightYear { get; set; } = DateTime.UtcNow.Year.ToString();
    public bool FooterShowPoweredBy { get; set; } = true;
    public string FooterCustomText { get; set; } = "";
    public string FooterText { get; set; } = "Building innovative solutions for modern businesses.";
    public string FooterHtml { get; set; } = "";
    public string CopyrightText { get; set; } = "";
    public bool ShowSocialLinks { get; set; } = true;
    public bool ShowFooterMenu { get; set; } = true;
    public bool StickyFooter { get; set; } = true;

    // Contact
    public string ContactEmail { get; set; } = "admin@example.com";
    public string ContactPhone { get; set; } = "";
    public string ContactAddress { get; set; } = "";
    public string Address { get; set; } = "";
    
    // Social Media
    public string FacebookUrl { get; set; } = "";
    public string TwitterUrl { get; set; } = "";
    public string LinkedInUrl { get; set; } = "";
    public string InstagramUrl { get; set; } = "";
    public string YouTubeUrl { get; set; } = "";
    public string GitHubUrl { get; set; } = "";

    // SEO
    public string MetaTitle { get; set; } = "Dynamic Role Menu System";
    public string MetaDescription { get; set; } = "A comprehensive role-based menu management system built with ASP.NET Core MVC framework";
    public string MetaKeywords { get; set; } = "role management, menu system, asp.net core, mvc, authorization";

    // System
    public string Version { get; set; } = "1.0.0";
    public bool MaintenanceMode { get; set; } = false;
    public bool AllowRegistration { get; set; } = false;
    public string DefaultUserRole { get; set; } = "User";

    // Helper properties for template compatibility
    public string GetFullTitle(string? pageTitle = null)
    {
        return !string.IsNullOrEmpty(pageTitle) ? $"{pageTitle} - {OrganizationName}" : OrganizationName;
    }

    public string GetCopyrightText()
    {
        return $"{FooterCopyrightYear} {FooterCompanyName}";
    }
}