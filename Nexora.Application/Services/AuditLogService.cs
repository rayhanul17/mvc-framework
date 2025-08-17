using System.Data;
using System.Text;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class AuditLogService : BaseService<Log>, IAuditLogService
{
    public AuditLogService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<DataTable>> GetAuditLogsForDataTableAsync(int start, int length, string? searchValue, string? sortColumn, string? sortDirection)
    {
        try
        {
            var sql = new StringBuilder();
            sql.Append(@"
                SELECT 
                    l.Id,
                    l.TableName,
                    l.EntityId,
                    l.Action,
                    l.Changes,
                    l.IpAddress,
                    l.UserAgent,
                    l.LoggedAt,
                    l.UserId,
                    u.UserName,
                    u.FullName
                FROM Logs l
                LEFT JOIN Users u ON l.UserId = u.Id
                WHERE 1=1
            ");

            var parameters = new List<object>();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                sql.Append(@" AND (
                    l.TableName LIKE CONCAT('%', @p0, '%') OR
                    l.Action LIKE CONCAT('%', @p0, '%') OR
                    l.Changes LIKE CONCAT('%', @p0, '%') OR
                    l.IpAddress LIKE CONCAT('%', @p0, '%') OR
                    u.UserName LIKE CONCAT('%', @p0, '%') OR
                    u.FullName LIKE CONCAT('%', @p0, '%')
                )");
                parameters.Add(searchValue);
            }

            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                var validColumns = new[] { "Id", "TableName", "EntityId", "Action", "LoggedAt", "UserName" };
                if (validColumns.Contains(sortColumn))
                {
                    sql.Append($" ORDER BY {sortColumn}");
                    if (!string.IsNullOrWhiteSpace(sortDirection) && sortDirection.ToLower() == "desc")
                    {
                        sql.Append(" DESC");
                    }
                }
                else
                {
                    sql.Append(" ORDER BY l.LoggedAt DESC");
                }
            }
            else
            {
                sql.Append(" ORDER BY l.LoggedAt DESC");
            }

            sql.Append($" LIMIT {length} OFFSET {start}");

            return await LoadDataTableAsync(sql.ToString(), parameters.ToArray());
        }
        catch (Exception ex)
        {
            return Result<DataTable>.Failure($"Error loading audit logs: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetTotalCountAsync()
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM Logs";
            var result = await LoadDataTableAsync(sql);
            
            if (result.IsSuccess && result.Data != null && result.Data.Rows.Count > 0)
            {
                var count = Convert.ToInt32(result.Data.Rows[0][0]);
                return Result<int>.Success(count);
            }
            
            return Result<int>.Failure("Unable to get total count");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error getting total count: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetFilteredCountAsync(string? searchValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                return await GetTotalCountAsync();
            }

            var sql = @"
                SELECT COUNT(*)
                FROM Logs l
                LEFT JOIN Users u ON l.UserId = u.Id
                WHERE 
                    l.TableName LIKE CONCAT('%', @p0, '%') OR
                    l.Action LIKE CONCAT('%', @p0, '%') OR
                    l.Changes LIKE CONCAT('%', @p0, '%') OR
                    l.IpAddress LIKE CONCAT('%', @p0, '%') OR
                    u.UserName LIKE CONCAT('%', @p0, '%') OR
                    u.FullName LIKE CONCAT('%', @p0, '%')
            ";

            var result = await LoadDataTableAsync(sql, searchValue);
            
            if (result.IsSuccess && result.Data != null && result.Data.Rows.Count > 0)
            {
                var count = Convert.ToInt32(result.Data.Rows[0][0]);
                return Result<int>.Success(count);
            }
            
            return Result<int>.Failure("Unable to get filtered count");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error getting filtered count: {ex.Message}");
        }
    }
}