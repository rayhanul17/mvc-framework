using System;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace MRCMS.Services.Interfaces
{
    public interface ISettingsService
    {
        // Core Settings Methods
        Task<string?> GetSettingAsync(string key);
        Task<T?> GetSettingAsync<T>(string key);
        Task SetSettingAsync(string key, string value, string? description = null, string? category = null);
        Task SetSettingAsync<T>(string key, T value, string? description = null, string? category = null);
        Task<bool> SettingExistsAsync(string key);
        Task RemoveSettingAsync(string key);
        
        // Timezone Settings
        Task<string> GetTimezoneAsync();
        Task SetTimezoneAsync(string timezoneId);
        Task<TimeZoneInfo> GetTimeZoneInfoAsync();
        
        // Date/Time Format Settings
        Task<string> GetShortDateFormatAsync();
        Task<string> GetLongDateFormatAsync();
        Task<string> GetShortTimeFormatAsync();
        Task<string> GetLongTimeFormatAsync();
        Task<string> GetDateTimeFormatAsync();
        
        // Conversion Methods
        DateTime ConvertFromUtc(DateTime utcDateTime);
        Task<DateTime> ConvertFromUtcAsync(DateTime utcDateTime);
        DateTime ConvertToUtc(DateTime localDateTime);
        Task<DateTime> ConvertToUtcAsync(DateTime localDateTime);
        
        // Format Methods
        string FormatDateTime(DateTime dateTime, bool convertFromUtc = true);
        string FormatDate(DateTime date, bool useLongFormat = false, bool convertFromUtc = true);
        string FormatTime(DateTime time, bool useLongFormat = false, bool convertFromUtc = true);
        Task<string> FormatDateTimeAsync(DateTime dateTime, bool convertFromUtc = true);
        Task<string> FormatDateAsync(DateTime date, bool useLongFormat = false, bool convertFromUtc = true);
        Task<string> FormatTimeAsync(DateTime time, bool useLongFormat = false, bool convertFromUtc = true);
        
        // Bulk Operations
        Task InitializeDefaultSettingsAsync();
        Task<Dictionary<string, string>> GetAllSettingsAsync(string? category = null);
    }
}