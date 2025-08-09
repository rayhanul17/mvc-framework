using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Core.Entities;

namespace DynamicRoleMenuSystem.Application.Interfaces;

public interface ILogService
{
    Task<Result<Log>> LogAsync(string tableName, int entityId, string action, 
        string? oldValues = null, string? newValues = null, string? changes = null);
        
    Task<Result<Log>> LogCreateAsync<T>(T entity) where T : BaseEntity;
    Task<Result<Log>> LogUpdateAsync<T>(T oldEntity, T newEntity) where T : BaseEntity;
    Task<Result<Log>> LogDeleteAsync<T>(T entity) where T : BaseEntity;
    
    Task<Result<IEnumerable<Log>>> GetLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null);
        
    Task<Result<IEnumerable<LogArchive>>> GetArchivedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null);
        
    Task<Result<IEnumerable<object>>> GetCombinedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        string source = "all", // "log", "archive", "all"
        int? skip = null,
        int? take = null);
        
    Task<Result<IEnumerable<string>>> GetTableNamesAsync();
    Task<Result> ArchiveOldLogsAsync();
    Task<Result> CleanupArchivedLogsAsync();
}