using MRCMS.Core.Models.Entities;
using MRCMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRCMS.Services.Interfaces
{
    public interface IAuditLogger
    {
        Task LogAsync(Log log);
        Task LogAsync(string level, string message, string exception = null, string properties = null);
        Task LogAsync(string entityName, string action, string details);
        Task LogInfoAsync(string message, string properties = null);
        Task LogWarningAsync(string message, string properties = null);
        Task LogErrorAsync(string message, string exception = null, string properties = null);
        
        // Query methods
        Task<List<AuditLogEntry>> GetLogsAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task ClearOldLogsAsync(int daysToKeep);
        Task<LoginStatistics> GetLoginStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<List<UserLoginCount>> GetUserLoginCountsAsync(DateTime startDate, DateTime endDate, int topCount);
    }
}