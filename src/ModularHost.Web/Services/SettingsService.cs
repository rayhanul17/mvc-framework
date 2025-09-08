using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MRCMS.Core.Infrastructure;
using MRCMS.Core.Models.Entities;
using MRCMS.Services.Interfaces;
using TimeZoneConverter;

namespace MRCMS.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SettingsService> _logger;
        private const string CacheKeyPrefix = "setting:";
        private const int CacheExpirationMinutes = 60;

        // Default Setting Keys
        public const string TimezoneKey = "System.Timezone";
        public const string ShortDateFormatKey = "System.ShortDateFormat";
        public const string LongDateFormatKey = "System.LongDateFormat";
        public const string ShortTimeFormatKey = "System.ShortTimeFormat";
        public const string LongTimeFormatKey = "System.LongTimeFormat";
        public const string DateTimeFormatKey = "System.DateTimeFormat";

        public SettingsService(AppDbContext context, IMemoryCache cache, ILogger<SettingsService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string?> GetSettingAsync(string key)
        {
            var cacheKey = $"{CacheKeyPrefix}{key}";
            
            if (_cache.TryGetValue<string>(cacheKey, out var cachedValue))
            {
                return cachedValue;
            }

            var setting = await _context.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == key && s.IsActive && !s.IsDeleted);

            if (setting != null)
            {
                _cache.Set(cacheKey, setting.Value, TimeSpan.FromMinutes(CacheExpirationMinutes));
                return setting.Value;
            }

            return null;
        }

        public async Task<T?> GetSettingAsync<T>(string key)
        {
            var value = await GetSettingAsync(key);
            if (string.IsNullOrEmpty(value))
                return default;

            try
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)value;
                
                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(value);
                
                if (typeof(T) == typeof(bool))
                    return (T)(object)bool.Parse(value);
                
                if (typeof(T) == typeof(DateTime))
                    return (T)(object)DateTime.Parse(value);
                
                if (typeof(T) == typeof(decimal))
                    return (T)(object)decimal.Parse(value);
                
                // For complex types, try JSON deserialization
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting setting value for key {Key} to type {Type}", key, typeof(T).Name);
                return default;
            }
        }

        public async Task SetSettingAsync(string key, string value, string? description = null, string? category = null)
        {
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                setting = new Setting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Description = description,
                    Category = category ?? "General",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
                
                if (description != null)
                    setting.Description = description;
                
                if (category != null)
                    setting.Category = category;
            }

            await _context.SaveChangesAsync();
            
            // Clear cache
            var cacheKey = $"{CacheKeyPrefix}{key}";
            _cache.Remove(cacheKey);
        }

        public async Task SetSettingAsync<T>(string key, T value, string? description = null, string? category = null)
        {
            string stringValue;
            
            if (value is string str)
                stringValue = str;
            else if (value is IConvertible)
                stringValue = value.ToString() ?? string.Empty;
            else
                stringValue = JsonSerializer.Serialize(value);

            await SetSettingAsync(key, stringValue, description, category);
        }

        public async Task<bool> SettingExistsAsync(string key)
        {
            return await _context.Settings
                .AnyAsync(s => s.Key == key && s.IsActive && !s.IsDeleted);
        }

        public async Task RemoveSettingAsync(string key)
        {
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting != null)
            {
                setting.IsDeleted = true;
                setting.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                // Clear cache
                var cacheKey = $"{CacheKeyPrefix}{key}";
                _cache.Remove(cacheKey);
            }
        }

        // Timezone Methods
        public async Task<string> GetTimezoneAsync()
        {
            return await GetSettingAsync(TimezoneKey) ?? "UTC";
        }

        public async Task SetTimezoneAsync(string timezoneId)
        {
            // Validate timezone ID
            try
            {
                var tzInfo = TZConvert.GetTimeZoneInfo(timezoneId);
                await SetSettingAsync(TimezoneKey, timezoneId, "System Timezone", "DateTime");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid timezone ID: {TimezoneId}", timezoneId);
                throw new ArgumentException($"Invalid timezone ID: {timezoneId}", nameof(timezoneId));
            }
        }

        public async Task<TimeZoneInfo> GetTimeZoneInfoAsync()
        {
            var timezoneId = await GetTimezoneAsync();
            try
            {
                return TZConvert.GetTimeZoneInfo(timezoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }

        // Date/Time Format Methods
        public async Task<string> GetShortDateFormatAsync()
        {
            return await GetSettingAsync(ShortDateFormatKey) ?? "MM/dd/yyyy";
        }

        public async Task<string> GetLongDateFormatAsync()
        {
            return await GetSettingAsync(LongDateFormatKey) ?? "MMMM dd, yyyy";
        }

        public async Task<string> GetShortTimeFormatAsync()
        {
            return await GetSettingAsync(ShortTimeFormatKey) ?? "h:mm tt";
        }

        public async Task<string> GetLongTimeFormatAsync()
        {
            return await GetSettingAsync(LongTimeFormatKey) ?? "h:mm:ss tt";
        }

        public async Task<string> GetDateTimeFormatAsync()
        {
            return await GetSettingAsync(DateTimeFormatKey) ?? "MM/dd/yyyy h:mm tt";
        }

        // Conversion Methods
        public DateTime ConvertFromUtc(DateTime utcDateTime)
        {
            var task = Task.Run(async () => await ConvertFromUtcAsync(utcDateTime));
            return task.Result;
        }

        public async Task<DateTime> ConvertFromUtcAsync(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            var timeZone = await GetTimeZoneInfoAsync();
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }

        public DateTime ConvertToUtc(DateTime localDateTime)
        {
            var task = Task.Run(async () => await ConvertToUtcAsync(localDateTime));
            return task.Result;
        }

        public async Task<DateTime> ConvertToUtcAsync(DateTime localDateTime)
        {
            var timeZone = await GetTimeZoneInfoAsync();
            
            if (localDateTime.Kind == DateTimeKind.Utc)
                return localDateTime;
            
            if (localDateTime.Kind == DateTimeKind.Unspecified)
                localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }

        // Format Methods
        public string FormatDateTime(DateTime dateTime, bool convertFromUtc = true)
        {
            var task = Task.Run(async () => await FormatDateTimeAsync(dateTime, convertFromUtc));
            return task.Result;
        }

        public string FormatDate(DateTime date, bool useLongFormat = false, bool convertFromUtc = true)
        {
            var task = Task.Run(async () => await FormatDateAsync(date, useLongFormat, convertFromUtc));
            return task.Result;
        }

        public string FormatTime(DateTime time, bool useLongFormat = false, bool convertFromUtc = true)
        {
            var task = Task.Run(async () => await FormatTimeAsync(time, useLongFormat, convertFromUtc));
            return task.Result;
        }

        public async Task<string> FormatDateTimeAsync(DateTime dateTime, bool convertFromUtc = true)
        {
            if (convertFromUtc)
                dateTime = await ConvertFromUtcAsync(dateTime);
            
            var format = await GetDateTimeFormatAsync();
            return dateTime.ToString(format);
        }

        public async Task<string> FormatDateAsync(DateTime date, bool useLongFormat = false, bool convertFromUtc = true)
        {
            if (convertFromUtc)
                date = await ConvertFromUtcAsync(date);
            
            var format = useLongFormat 
                ? await GetLongDateFormatAsync() 
                : await GetShortDateFormatAsync();
            
            return date.ToString(format);
        }

        public async Task<string> FormatTimeAsync(DateTime time, bool useLongFormat = false, bool convertFromUtc = true)
        {
            if (convertFromUtc)
                time = await ConvertFromUtcAsync(time);
            
            var format = useLongFormat 
                ? await GetLongTimeFormatAsync() 
                : await GetShortTimeFormatAsync();
            
            return time.ToString(format);
        }

        // Initialize Default Settings
        public async Task InitializeDefaultSettingsAsync()
        {
            var defaultSettings = new Dictionary<string, (string Value, string Description, string Category)>
            {
                [TimezoneKey] = ("UTC", "System Timezone", "DateTime"),
                [ShortDateFormatKey] = ("MM/dd/yyyy", "Short date format", "DateTime"),
                [LongDateFormatKey] = ("MMMM dd, yyyy", "Long date format", "DateTime"),
                [ShortTimeFormatKey] = ("h:mm tt", "Short time format", "DateTime"),
                [LongTimeFormatKey] = ("h:mm:ss tt", "Long time format", "DateTime"),
                [DateTimeFormatKey] = ("MM/dd/yyyy h:mm tt", "Date and time format", "DateTime"),
                
                // Additional default settings
                ["System.SiteName"] = ("ModularHost", "Site name", "General"),
                ["System.PageSize"] = ("10", "Default page size for lists", "Display"),
                ["System.MaxUploadSize"] = ("10485760", "Maximum file upload size in bytes", "Files"),
                ["System.AllowedFileExtensions"] = (".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx", "Allowed file extensions", "Files"),
                ["System.EnableAuditLog"] = ("true", "Enable audit logging", "Security"),
                ["System.SessionTimeout"] = ("20", "Session timeout in minutes", "Security"),
                ["System.PasswordMinLength"] = ("8", "Minimum password length", "Security"),
                ["System.RequireUppercase"] = ("true", "Require uppercase in password", "Security"),
                ["System.RequireLowercase"] = ("true", "Require lowercase in password", "Security"),
                ["System.RequireDigit"] = ("true", "Require digit in password", "Security"),
                ["System.RequireSpecialCharacter"] = ("true", "Require special character in password", "Security")
            };

            foreach (var (key, (value, description, category)) in defaultSettings)
            {
                if (!await SettingExistsAsync(key))
                {
                    await SetSettingAsync(key, value, description, category);
                }
            }
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync(string? category = null)
        {
            var query = _context.Settings
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDeleted);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(s => s.Category == category);

            var settings = await query
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }
    }
}