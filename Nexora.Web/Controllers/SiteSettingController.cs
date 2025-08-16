using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using Nexora.Web.Models.ViewModels;
using System.Text.Json;

namespace Nexora.Web.Controllers;

[Authorize]
public class SiteSettingController : BaseController
{
    private readonly ISiteSettingService _siteSettingService;

    public SiteSettingController(ISiteSettingService siteSettingService)
    {
        _siteSettingService = siteSettingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(SettingCategory? category = null)
    {
        var result = category.HasValue 
            ? await _siteSettingService.GetSettingsByCategoryAsync(category.Value)
            : await _siteSettingService.GetAllSettingsAsync();

        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error loading site settings");
            return View(new List<SiteSettingCategoryViewModel>());
        }

        // Group settings by category
        var groupedSettings = result.Data!
            .GroupBy(s => s.Category)
            .Select(g => new SiteSettingCategoryViewModel
            {
                Category = g.Key,
                CategoryName = g.Key.ToString(),
                Settings = g.Select(s => new SiteSettingViewModel
                {
                    Id = s.Id,
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description,
                    Category = s.Category,
                    Type = s.Type,
                    ValidValues = s.ValidValues,
                    IsRequired = s.IsRequired,
                    IsSystemSetting = s.IsSystemSetting,
                    Order = s.Order,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    CreatedBy = s.CreatedBy,
                    UpdatedBy = s.UpdatedBy
                }).OrderBy(s => s.Order).ThenBy(s => s.Key).ToList()
            })
            .OrderBy(c => c.Category)
            .ToList();

        ViewBag.Categories = Enum.GetValues<SettingCategory>();
        ViewBag.SelectedCategory = category;

        return View(groupedSettings);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _siteSettingService.GetByIdAsync(id);
        if (!result.IsSuccess || result.Data == null)
        {
            return NotFound();
        }

        var setting = result.Data;
        var viewModel = new SiteSettingViewModel
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category,
            Type = setting.Type,
            ValidValues = setting.ValidValues,
            IsRequired = setting.IsRequired,
            IsSystemSetting = setting.IsSystemSetting,
            Order = setting.Order,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt,
            CreatedBy = setting.CreatedBy,
            UpdatedBy = setting.UpdatedBy
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return RedirectToAction(nameof(CreateEdit));
    }

    public IActionResult Edit(int id)
    {
        return RedirectToAction(nameof(CreateEdit), new { id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CreateEdit(int? id = null)
    {
        var isEditMode = id.HasValue && id.Value > 0;
        
        if (isEditMode)
        {
            var result = await _siteSettingService.GetByIdAsync(id!.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var setting = result.Data;
            var viewModel = new SiteSettingCreateEditViewModel
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                Category = setting.Category,
                Type = setting.Type,
                ValidValues = setting.ValidValues,
                IsRequired = setting.IsRequired,
                IsSystemSetting = setting.IsSystemSetting,
                Order = setting.Order,
                CreatedAt = setting.CreatedAt,
                CreatedBy = setting.CreatedBy,
                IsEditMode = true
            };

            ViewBag.Categories = Enum.GetValues<SettingCategory>();
            ViewBag.Types = Enum.GetValues<SettingType>();
            return View("CreateEdit", viewModel);
        }
        else
        {
            ViewBag.Categories = Enum.GetValues<SettingCategory>();
            ViewBag.Types = Enum.GetValues<SettingType>();
            return View("CreateEdit", new SiteSettingCreateEditViewModel { IsEditMode = false });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CreateEdit(SiteSettingCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = Enum.GetValues<SettingCategory>();
            ViewBag.Types = Enum.GetValues<SettingType>();
            return View("CreateEdit", model);
        }

        // Validate JSON if provided
        if (!string.IsNullOrEmpty(model.ValidValues))
        {
            try
            {
                JsonDocument.Parse(model.ValidValues);
            }
            catch
            {
                ModelState.AddModelError(nameof(model.ValidValues), "Valid Values must be valid JSON");
                ViewBag.Categories = Enum.GetValues<SettingCategory>();
                ViewBag.Types = Enum.GetValues<SettingType>();
                return View("CreateEdit", model);
            }
        }

        if (model.Id == 0) // Create mode
        {
            var setting = new SiteSetting
            {
                Key = model.Key,
                Value = model.Value,
                Description = model.Description,
                Category = model.Category,
                Type = model.Type,
                ValidValues = model.ValidValues,
                IsRequired = model.IsRequired,
                IsSystemSetting = model.IsSystemSetting,
                Order = model.Order,
                CreatedBy = User.Identity?.Name
            };

            var result = await _siteSettingService.CreateSettingAsync(setting);
            if (!result.IsSuccess)
            {
                AddErrorsToModelState(result);
                ViewBag.Categories = Enum.GetValues<SettingCategory>();
                ViewBag.Types = Enum.GetValues<SettingType>();
                return View("CreateEdit", model);
            }

            SetSuccessMessage("Site setting created successfully");
            return RedirectToAction(nameof(Index), new { category = model.Category });
        }
        else // Edit mode
        {
            var result = await _siteSettingService.GetByIdAsync(model.Id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var setting = result.Data;
            setting.Key = model.Key;
            setting.Value = model.Value;
            setting.Description = model.Description;
            setting.Category = model.Category;
            setting.Type = model.Type;
            setting.ValidValues = model.ValidValues;
            setting.IsRequired = model.IsRequired;
            setting.IsSystemSetting = model.IsSystemSetting;
            setting.Order = model.Order;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = User.Identity?.Name;

            var updateResult = await _siteSettingService.UpdateAsync(setting);
            if (!updateResult.IsSuccess)
            {
                AddErrorsToModelState(updateResult);
                ViewBag.Categories = Enum.GetValues<SettingCategory>();
                ViewBag.Types = Enum.GetValues<SettingType>();
                return View("CreateEdit", model);
            }

            SetSuccessMessage("Site setting updated successfully");
            return RedirectToAction(nameof(Index), new { category = model.Category });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _siteSettingService.DeleteSettingAsync(id);
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error deleting setting");
        }
        else
        {
            SetSuccessMessage("Site setting deleted successfully");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> BulkEdit(SettingCategory category)
    {
        var result = await _siteSettingService.GetSettingsByCategoryAsync(category);
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error loading settings");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new BulkUpdateSettingsViewModel
        {
            Settings = result.Data!.Select(s => new BulkSettingItem
            {
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                Type = s.Type,
                ValidValues = s.ValidValues,
                IsRequired = s.IsRequired
            }).ToList()
        };

        ViewBag.Category = category;
        ViewBag.CategoryName = category.ToString();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkEdit(SettingCategory category, BulkUpdateSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Category = category;
            ViewBag.CategoryName = category.ToString();
            return View(model);
        }

        var settingsDict = model.Settings.ToDictionary(s => s.Key, s => s.Value);
        var result = await _siteSettingService.BulkUpdateSettingsAsync(settingsDict, User.Identity?.Name);

        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error updating settings");
            ViewBag.Category = category;
            ViewBag.CategoryName = category.ToString();
            return View(model);
        }

        SetSuccessMessage("Settings updated successfully");
        return RedirectToAction(nameof(Index), new { category });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ResetToDefaults()
    {
        var result = await _siteSettingService.ResetToDefaultsAsync();
        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error resetting settings");
        }
        else
        {
            SetSuccessMessage("Settings reset to defaults successfully");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(SettingCategory? category = null)
    {
        var result = category.HasValue 
            ? await _siteSettingService.GetSettingsAsDictionaryAsync(category.Value)
            : await _siteSettingService.GetSettingsAsDictionaryAsync();

        if (!result.IsSuccess)
        {
            SetErrorMessage(result.ErrorMessage ?? "Error exporting settings");
            return RedirectToAction(nameof(Index));
        }

        var json = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
        var fileName = category.HasValue ? $"settings-{category.Value}.json" : "settings-all.json";
        
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    [HttpGet]
    public async Task<JsonResult> GetSettingValue(string key)
    {
        var result = await _siteSettingService.GetSettingValueAsync(key);
        return Json(new { success = result.IsSuccess, value = result.Data });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> UpdateSettingValue(string key, string value)
    {
        var result = await _siteSettingService.UpdateSettingValueAsync(key, value, User.Identity?.Name);
        return Json(new { success = result.IsSuccess, message = result.ErrorMessage });
    }

    [HttpGet]
    public async Task<IActionResult> Branding()
    {
        var result = await _siteSettingService.GetSettingsByCategoryAsync(SettingCategory.Branding);
        var themeResult = await _siteSettingService.GetSettingsByCategoryAsync(SettingCategory.Theme);
        
        var allSettings = new List<SiteSetting>();
        if (result.IsSuccess && result.Data != null)
            allSettings.AddRange(result.Data);
        if (themeResult.IsSuccess && themeResult.Data != null)
            allSettings.AddRange(themeResult.Data);
        
        return View(allSettings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBranding(
        string OrganizationName, 
        string ShortName, 
        string? Slogan,
        IFormFile? SmallLogoFile,
        IFormFile? LargeLogoFile,
        IFormFile? FaviconFile,
        string PrimaryColor,
        string SecondaryColor,
        bool DarkModeDefault = false)
    {
        try
        {
            var updates = new Dictionary<string, string>
            {
                { "OrganizationName", OrganizationName },
                { "ShortName", ShortName },
                { "Slogan", Slogan ?? string.Empty },
                { "PrimaryColor", PrimaryColor },
                { "SecondaryColor", SecondaryColor },
                { "DarkModeDefault", DarkModeDefault.ToString().ToLower() }
            };

            // Handle file uploads
            if (SmallLogoFile != null && SmallLogoFile.Length > 0)
            {
                var smallLogoPath = await SaveUploadedFile(SmallLogoFile, "logos");
                if (!string.IsNullOrEmpty(smallLogoPath))
                    updates["SmallLogo"] = smallLogoPath;
            }

            if (LargeLogoFile != null && LargeLogoFile.Length > 0)
            {
                var largeLogoPath = await SaveUploadedFile(LargeLogoFile, "logos");
                if (!string.IsNullOrEmpty(largeLogoPath))
                    updates["LargeLogo"] = largeLogoPath;
            }

            if (FaviconFile != null && FaviconFile.Length > 0)
            {
                var faviconPath = await SaveUploadedFile(FaviconFile, "");
                if (!string.IsNullOrEmpty(faviconPath))
                    updates["FaviconPath"] = faviconPath;
            }

            var result = await _siteSettingService.BulkUpdateSettingsAsync(updates, User.Identity?.Name);
            
            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Branding settings saved successfully" });
            }
            
            return Json(new { success = false, message = result.ErrorMessage ?? "Failed to save branding settings" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Footer()
    {
        var result = await _siteSettingService.GetSettingValueAsync("FooterHtml");
        
        SiteSetting footerSetting = null;
        if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
        {
            footerSetting = new SiteSetting { Key = "FooterHtml", Value = result.Data };
        }
        
        return View(footerSetting);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFooter(string FooterHtml, bool StickyFooter = true)
    {
        try
        {
            var updates = new Dictionary<string, string>
            {
                { "FooterHtml", FooterHtml ?? string.Empty },
                { "StickyFooter", StickyFooter.ToString().ToLower() }
            };

            var result = await _siteSettingService.BulkUpdateSettingsAsync(updates, User.Identity?.Name);
            
            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Footer settings saved successfully" });
            }
            
            return Json(new { success = false, message = result.ErrorMessage ?? "Failed to save footer settings" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    private async Task<string?> SaveUploadedFile(IFormFile file, string subfolder)
    {
        try
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{subfolder}/{uniqueFileName}".Replace("//", "/");
        }
        catch
        {
            return null;
        }
    }
}