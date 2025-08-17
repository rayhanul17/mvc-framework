using Nexora.Core.Common;
using Nexora.Core.Entities;

namespace Nexora.Application.Interfaces;

public interface ILogService
{
    // Core logging method
    Task<Result<Log>> LogAsync(string tableName, int entityId, string action, 
        string? oldValues = null, string? newValues = null, string? changes = null);
    
    // Methods used by LogController    
    Task<Result<IEnumerable<object>>> GetCombinedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        string source = "all", // "log", "archive", "all"
        int? skip = null,
        int? take = null,
        string? ipAddress = null,
        string? keyword = null);
        
    Task<Result<IEnumerable<string>>> GetTableNamesAsync();
    
    Task<Result<Log>> GetLogByIdAsync(int id, string source = "log");
    
    // Methods used by LogArchiveBackgroundService
    Task<Result> ArchiveOldLogsAsync();
    Task<Result> CleanupArchivedLogsAsync();
}