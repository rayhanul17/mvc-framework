using Nexora.Core.Common;
using Nexora.Core.Entities;
using System.Data;

namespace Nexora.Application.Interfaces;

public interface IAuditLogService : IBaseService<Log>
{
    Task<Result<DataTable>> GetAuditLogsForDataTableAsync(int start, int length, string? searchValue, string? sortColumn, string? sortDirection);
    Task<Result<int>> GetTotalCountAsync();
    Task<Result<int>> GetFilteredCountAsync(string? searchValue);
}