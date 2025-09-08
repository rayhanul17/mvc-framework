using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MRCMS.Services.Interfaces;

namespace MRCMS.Core.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts UTC DateTime to local timezone and formats it
        /// </summary>
        public static string ToLocalTime(this DateTime utcDateTime, IHtmlHelper html, bool useLongFormat = false)
        {
            var settingsService = html.ViewContext.HttpContext.RequestServices.GetService<ISettingsService>();
            if (settingsService == null)
                return utcDateTime.ToString();

            return settingsService.FormatDateTime(utcDateTime);
        }

        /// <summary>
        /// Formats DateTime according to system settings
        /// </summary>
        public static HtmlString FormatDateTime(this IHtmlHelper html, DateTime dateTime, bool convertFromUtc = true)
        {
            var settingsService = html.ViewContext.HttpContext.RequestServices.GetService<ISettingsService>();
            if (settingsService == null)
                return new HtmlString(dateTime.ToString());

            var formatted = settingsService.FormatDateTime(dateTime, convertFromUtc);
            return new HtmlString(formatted);
        }

        /// <summary>
        /// Formats Date according to system settings
        /// </summary>
        public static HtmlString FormatDate(this IHtmlHelper html, DateTime date, bool useLongFormat = false, bool convertFromUtc = true)
        {
            var settingsService = html.ViewContext.HttpContext.RequestServices.GetService<ISettingsService>();
            if (settingsService == null)
                return new HtmlString(date.ToShortDateString());

            var formatted = settingsService.FormatDate(date, useLongFormat, convertFromUtc);
            return new HtmlString(formatted);
        }

        /// <summary>
        /// Formats Time according to system settings
        /// </summary>
        public static HtmlString FormatTime(this IHtmlHelper html, DateTime time, bool useLongFormat = false, bool convertFromUtc = true)
        {
            var settingsService = html.ViewContext.HttpContext.RequestServices.GetService<ISettingsService>();
            if (settingsService == null)
                return new HtmlString(time.ToShortTimeString());

            var formatted = settingsService.FormatTime(time, useLongFormat, convertFromUtc);
            return new HtmlString(formatted);
        }

        /// <summary>
        /// Gets relative time string (e.g., "2 hours ago", "yesterday", "3 days ago")
        /// </summary>
        public static HtmlString RelativeTime(this IHtmlHelper html, DateTime dateTime, bool convertFromUtc = true)
        {
            var settingsService = html.ViewContext.HttpContext.RequestServices.GetService<ISettingsService>();
            if (settingsService != null && convertFromUtc)
            {
                dateTime = settingsService.ConvertFromUtc(dateTime);
            }

            var now = DateTime.Now;
            var timeSpan = now - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return new HtmlString("just now");
            
            if (timeSpan.TotalMinutes < 60)
            {
                var minutes = (int)timeSpan.TotalMinutes;
                return new HtmlString($"{minutes} minute{(minutes != 1 ? "s" : "")} ago");
            }
            
            if (timeSpan.TotalHours < 24)
            {
                var hours = (int)timeSpan.TotalHours;
                return new HtmlString($"{hours} hour{(hours != 1 ? "s" : "")} ago");
            }
            
            if (timeSpan.TotalDays < 2)
                return new HtmlString("yesterday");
            
            if (timeSpan.TotalDays < 7)
            {
                var days = (int)timeSpan.TotalDays;
                return new HtmlString($"{days} days ago");
            }
            
            if (timeSpan.TotalDays < 30)
            {
                var weeks = (int)(timeSpan.TotalDays / 7);
                return new HtmlString($"{weeks} week{(weeks != 1 ? "s" : "")} ago");
            }
            
            if (timeSpan.TotalDays < 365)
            {
                var months = (int)(timeSpan.TotalDays / 30);
                return new HtmlString($"{months} month{(months != 1 ? "s" : "")} ago");
            }
            
            var years = (int)(timeSpan.TotalDays / 365);
            return new HtmlString($"{years} year{(years != 1 ? "s" : "")} ago");
        }

        /// <summary>
        /// Displays date with tooltip showing full datetime
        /// </summary>
        public static HtmlString DateWithTooltip(this IHtmlHelper html, DateTime dateTime, bool convertFromUtc = true)
        {
            var settingsService = html.ViewContext.HttpContext.RequestServices.GetService<ISettingsService>();
            if (settingsService == null)
                return new HtmlString(dateTime.ToString());

            var displayDate = settingsService.FormatDate(dateTime, false, convertFromUtc);
            var fullDateTime = settingsService.FormatDateTime(dateTime, convertFromUtc);
            
            var htmlString = $"<span title=\"{fullDateTime}\" data-toggle=\"tooltip\">{displayDate}</span>";
            return new HtmlString(htmlString);
        }

        /// <summary>
        /// Extension method for DateTime to convert to UTC
        /// </summary>
        public static DateTime ToUtc(this DateTime localDateTime, TimeZoneInfo? timeZone = null)
        {
            if (localDateTime.Kind == DateTimeKind.Utc)
                return localDateTime;

            timeZone ??= TimeZoneInfo.Local;
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }

        /// <summary>
        /// Extension method for DateTime to convert from UTC
        /// </summary>
        public static DateTime FromUtc(this DateTime utcDateTime, TimeZoneInfo? timeZone = null)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            timeZone ??= TimeZoneInfo.Local;
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
    }
}