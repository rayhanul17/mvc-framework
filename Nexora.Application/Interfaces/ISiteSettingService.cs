using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface ISiteSettingService : IBaseService<SiteSetting>
{
    Task<Result<IEnumerable<SiteSetting>>> GetAllSettingsAsync();
    Task<Result<IEnumerable<SiteSetting>>> GetSettingsByCategoryAsync(SettingCategory category);
    Task<Result<SiteSetting>> GetSettingByKeyAsync(string key);
    Task<Result<string>> GetSettingValueAsync(string key, string defaultValue = "");
    Task<Result<T>> GetSettingValueAsync<T>(string key, T defaultValue = default(T));
    Task<Result<bool>> UpdateSettingValueAsync(string key, string value, string? updatedBy = null);
    Task<Result<Dictionary<string, string>>> GetSettingsAsDictionaryAsync();
    Task<Result<Dictionary<string, string>>> GetSettingsAsDictionaryAsync(SettingCategory category);
    Task<Result<bool>> BulkUpdateSettingsAsync(Dictionary<string, string> settings, string? updatedBy = null);
    Task<Result<SiteSetting>> CreateSettingAsync(SiteSetting setting);
    Task<Result<bool>> DeleteSettingAsync(int id);
    Task<Result<bool>> ResetToDefaultsAsync();
    Task<Result<IEnumerable<SettingCategory>>> GetUsedCategoriesAsync();
}