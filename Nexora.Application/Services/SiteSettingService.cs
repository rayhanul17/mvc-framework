using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class SiteSettingService : BaseService<SiteSetting>, ISiteSettingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SiteSettingService> _logger;
    private const string CACHE_KEY_PREFIX = "SiteSetting:";
    private const string ALL_SETTINGS_CACHE_KEY = "SiteSettings:All";
    private const int CACHE_EXPIRATION_MINUTES = 30;

    public SiteSettingService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<SiteSettingService> logger)
        : base(unitOfWork)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<SiteSetting>>> GetAllSettingsAsync()
    {
        try
        {
            if (_cache.TryGetValue(ALL_SETTINGS_CACHE_KEY, out IEnumerable<SiteSetting>? cachedSettings) && cachedSettings != null)
            {
                return Result<IEnumerable<SiteSetting>>.Success(cachedSettings);
            }

            var allSettings = await _unitOfWork.Repository<SiteSetting>()
                .FindAsync(s => true);
            
            var settings = allSettings.OrderBy(x => x.Category).ThenBy(x => x.Order).ThenBy(x => x.Key);

            _cache.Set(ALL_SETTINGS_CACHE_KEY, settings, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));

            return Result<IEnumerable<SiteSetting>>.Success(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all site settings");
            return Result<IEnumerable<SiteSetting>>.Failure($"Error retrieving settings: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SiteSetting>>> GetSettingsByCategoryAsync(SettingCategory category)
    {
        try
        {
            var allSettings = await _unitOfWork.Repository<SiteSetting>()
                .FindAsync(s => s.Category == category);
            
            var settings = allSettings.OrderBy(x => x.Order).ThenBy(x => x.Key);

            return Result<IEnumerable<SiteSetting>>.Success(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings for category {Category}", category);
            return Result<IEnumerable<SiteSetting>>.Failure($"Error retrieving settings: {ex.Message}");
        }
    }

    public async Task<Result<SiteSetting>> GetSettingByKeyAsync(string key)
    {
        try
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{key}";
            if (_cache.TryGetValue(cacheKey, out SiteSetting? cachedSetting) && cachedSetting != null)
            {
                return Result<SiteSetting>.Success(cachedSetting);
            }

            var setting = await _unitOfWork.Repository<SiteSetting>()
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
                return Result<SiteSetting>.Failure($"Setting with key '{key}' not found");

            _cache.Set(cacheKey, setting, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES));
            return Result<SiteSetting>.Success(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting with key {Key}", key);
            return Result<SiteSetting>.Failure($"Error retrieving setting: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetSettingValueAsync(string key, string defaultValue = "")
    {
        try
        {
            var settingResult = await GetSettingByKeyAsync(key);
            return settingResult.IsSuccess 
                ? Result<string>.Success(settingResult.Data!.Value) 
                : Result<string>.Success(defaultValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting value for key {Key}", key);
            return Result<string>.Success(defaultValue);
        }
    }

    public async Task<Result<T>> GetSettingValueAsync<T>(string key, T defaultValue = default(T))
    {
        try
        {
            var settingResult = await GetSettingByKeyAsync(key);
            if (!settingResult.IsSuccess)
                return Result<T>.Success(defaultValue!);

            var value = settingResult.Data!.Value;
            if (string.IsNullOrEmpty(value))
                return Result<T>.Success(defaultValue!);

            // Handle different types
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(value, out var boolValue))
                    return Result<T>.Success((T)(object)boolValue);
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(value, out var intValue))
                    return Result<T>.Success((T)(object)intValue);
            }
            else if (typeof(T) == typeof(decimal))
            {
                if (decimal.TryParse(value, out var decimalValue))
                    return Result<T>.Success((T)(object)decimalValue);
            }
            else if (typeof(T) == typeof(string))
            {
                return Result<T>.Success((T)(object)value);
            }

            // Try JSON deserialization for complex types
            try
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(value);
                return Result<T>.Success(deserializedValue!);
            }
            catch
            {
                return Result<T>.Success(defaultValue!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting value for key {Key}", key);
            return Result<T>.Success(defaultValue!);
        }
    }

    public async Task<Result<bool>> UpdateSettingValueAsync(string key, string value, string? updatedBy = null)
    {
        try
        {
            var setting = await _unitOfWork.Repository<SiteSetting>()
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
                return Result<bool>.Failure($"Setting with key '{key}' not found");

            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;

            _unitOfWork.Repository<SiteSetting>().Update(setting);
            await _unitOfWork.SaveChangesAsync();

            // Clear cache
            _cache.Remove($"{CACHE_KEY_PREFIX}{key}");
            _cache.Remove(ALL_SETTINGS_CACHE_KEY);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating setting value for key {Key}", key);
            return Result<bool>.Failure($"Error updating setting: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, string>>> GetSettingsAsDictionaryAsync()
    {
        try
        {
            var settingsResult = await GetAllSettingsAsync();
            if (!settingsResult.IsSuccess)
                return Result<Dictionary<string, string>>.Failure(settingsResult.ErrorMessage);

            var dictionary = settingsResult.Data!.ToDictionary(s => s.Key, s => s.Value);
            return Result<Dictionary<string, string>>.Success(dictionary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings as dictionary");
            return Result<Dictionary<string, string>>.Failure($"Error retrieving settings: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, string>>> GetSettingsAsDictionaryAsync(SettingCategory category)
    {
        try
        {
            var settingsResult = await GetSettingsByCategoryAsync(category);
            if (!settingsResult.IsSuccess)
                return Result<Dictionary<string, string>>.Failure(settingsResult.ErrorMessage);

            var dictionary = settingsResult.Data!.ToDictionary(s => s.Key, s => s.Value);
            return Result<Dictionary<string, string>>.Success(dictionary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings as dictionary for category {Category}", category);
            return Result<Dictionary<string, string>>.Failure($"Error retrieving settings: {ex.Message}");
        }
    }

    public async Task<Result<bool>> BulkUpdateSettingsAsync(Dictionary<string, string> settings, string? updatedBy = null)
    {
        try
        {
            var existingSettings = await _unitOfWork.Repository<SiteSetting>()
                .FindAsync(s => settings.Keys.Contains(s.Key));

            foreach (var setting in existingSettings)
            {
                if (settings.TryGetValue(setting.Key, out var newValue))
                {
                    setting.Value = newValue;
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.UpdatedBy = updatedBy;
                    _unitOfWork.Repository<SiteSetting>().Update(setting);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Clear all cache
            _cache.Remove(ALL_SETTINGS_CACHE_KEY);
            foreach (var key in settings.Keys)
            {
                _cache.Remove($"{CACHE_KEY_PREFIX}{key}");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating settings");
            return Result<bool>.Failure($"Error updating settings: {ex.Message}");
        }
    }

    public async Task<Result<SiteSetting>> CreateSettingAsync(SiteSetting setting)
    {
        try
        {
            // Check if key already exists
            var existing = await _unitOfWork.Repository<SiteSetting>()
                .FirstOrDefaultAsync(s => s.Key == setting.Key);

            if (existing != null)
                return Result<SiteSetting>.Failure($"Setting with key '{setting.Key}' already exists");

            setting.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<SiteSetting>().AddAsync(setting);
            await _unitOfWork.SaveChangesAsync();

            // Clear cache
            _cache.Remove(ALL_SETTINGS_CACHE_KEY);

            return Result<SiteSetting>.Success(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating setting with key {Key}", setting.Key);
            return Result<SiteSetting>.Failure($"Error creating setting: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteSettingAsync(int id)
    {
        try
        {
            var setting = await _unitOfWork.Repository<SiteSetting>().GetByIdAsync(id);
            if (setting == null)
                return Result<bool>.Failure("Setting not found");

            if (setting.IsSystemSetting)
                return Result<bool>.Failure("Cannot delete system settings");

            _unitOfWork.Repository<SiteSetting>().Remove(setting);
            await _unitOfWork.SaveChangesAsync();

            // Clear cache
            _cache.Remove($"{CACHE_KEY_PREFIX}{setting.Key}");
            _cache.Remove(ALL_SETTINGS_CACHE_KEY);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting with ID {Id}", id);
            return Result<bool>.Failure($"Error deleting setting: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ResetToDefaultsAsync()
    {
        try
        {
            // This would implement logic to reset all settings to their default values
            // For now, we'll just clear the cache to force reload
            _cache.Remove(ALL_SETTINGS_CACHE_KEY);
            
            var allSettings = await _unitOfWork.Repository<SiteSetting>().GetAllAsync();
            foreach (var setting in allSettings)
            {
                _cache.Remove($"{CACHE_KEY_PREFIX}{setting.Key}");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting settings to defaults");
            return Result<bool>.Failure($"Error resetting settings: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<SettingCategory>>> GetUsedCategoriesAsync()
    {
        try
        {
            var settings = await _unitOfWork.Repository<SiteSetting>().GetAllAsync();
            var categories = settings.Select(s => s.Category).Distinct().OrderBy(c => c);
            return Result<IEnumerable<SettingCategory>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving used categories");
            return Result<IEnumerable<SettingCategory>>.Failure($"Error retrieving categories: {ex.Message}");
        }
    }
}