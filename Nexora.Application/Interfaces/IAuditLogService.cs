using Nexora.Core.Common;
using Nexora.Core.Entities;
using System.Data;

namespace Nexora.Application.Interfaces;

public interface IAuditLogService : IBaseService<Log>
{
    Task<Result<DataTable>> GetAuditLogsForDataTableAsync(int start, int length, string? searchValue, string? sortColumn, string? sortDirection, string? tableName = null, string? action = null, string? userName = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<int>> GetTotalCountAsync();
    Task<Result<int>> GetFilteredCountAsync(string? searchValue, string? tableName = null, string? action = null, string? userName = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<string>>> GetUniqueTableNamesAsync();
}