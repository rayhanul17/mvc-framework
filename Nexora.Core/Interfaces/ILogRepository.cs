using Nexora.Core.Entities;
using System.Linq.Expressions;

namespace Nexora.Core.Interfaces;

public interface ILogRepository : IBaseRepository<Log>
{
    Task<IEnumerable<Log>> GetLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null);
    
    Task<IEnumerable<LogArchive>> GetArchivedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null);
    
    Task<IEnumerable<object>> GetCombinedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        bool includeArchived = false,
        int? skip = null,
        int? take = null);
    
    Task<int> GetLogCountAsync(Expression<Func<Log, bool>>? predicate = null);
    Task<int> GetArchivedLogCountAsync(Expression<Func<LogArchive, bool>>? predicate = null);
    
    Task<IEnumerable<string>> GetDistinctTableNamesAsync();
    Task ArchiveLogsAsync(DateTime olderThan);
    Task DeleteOldArchivedLogsAsync(DateTime olderThan);
}