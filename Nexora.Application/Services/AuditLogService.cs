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

    public async Task<Result<DataTable>> GetAuditLogsForDataTableAsync(int start, int length, string? searchValue, string? sortColumn, string? sortDirection, string? tableName = null, string? action = null, string? userName = null, DateTime? startDate = null, DateTime? endDate = null)
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
            var paramIndex = 0;

            // Add filter conditions
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                sql.Append($" AND l.TableName = @p{paramIndex}");
                parameters.Add(tableName);
                paramIndex++;
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                sql.Append($" AND l.Action = @p{paramIndex}");
                parameters.Add(action);
                paramIndex++;
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                sql.Append($" AND (u.UserName LIKE CONCAT('%', @p{paramIndex}, '%') OR u.FullName LIKE CONCAT('%', @p{paramIndex}, '%'))");
                parameters.Add(userName);
                paramIndex++;
            }

            if (startDate.HasValue)
            {
                sql.Append($" AND l.LoggedAt >= @p{paramIndex}");
                parameters.Add(startDate.Value);
                paramIndex++;
            }

            if (endDate.HasValue)
            {
                sql.Append($" AND l.LoggedAt <= @p{paramIndex}");
                parameters.Add(endDate.Value.AddDays(1).AddSeconds(-1)); // Include end of day
                paramIndex++;
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                sql.Append($@" AND (
                    l.TableName LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    l.Action LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    l.Changes LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    l.IpAddress LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    u.UserName LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    u.FullName LIKE CONCAT('%', @p{paramIndex}, '%')
                )");
                parameters.Add(searchValue);
                paramIndex++;
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

    public async Task<Result<int>> GetFilteredCountAsync(string? searchValue, string? tableName = null, string? action = null, string? userName = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var sql = new StringBuilder();
            sql.Append(@"
                SELECT COUNT(*)
                FROM Logs l
                LEFT JOIN Users u ON l.UserId = u.Id
                WHERE 1=1
            ");

            var parameters = new List<object>();
            var paramIndex = 0;

            // Add filter conditions
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                sql.Append($" AND l.TableName = @p{paramIndex}");
                parameters.Add(tableName);
                paramIndex++;
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                sql.Append($" AND l.Action = @p{paramIndex}");
                parameters.Add(action);
                paramIndex++;
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                sql.Append($" AND (u.UserName LIKE CONCAT('%', @p{paramIndex}, '%') OR u.FullName LIKE CONCAT('%', @p{paramIndex}, '%'))");
                parameters.Add(userName);
                paramIndex++;
            }

            if (startDate.HasValue)
            {
                sql.Append($" AND l.LoggedAt >= @p{paramIndex}");
                parameters.Add(startDate.Value);
                paramIndex++;
            }

            if (endDate.HasValue)
            {
                sql.Append($" AND l.LoggedAt <= @p{paramIndex}");
                parameters.Add(endDate.Value.AddDays(1).AddSeconds(-1)); // Include end of day
                paramIndex++;
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                sql.Append($@" AND (
                    l.TableName LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    l.Action LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    l.Changes LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    l.IpAddress LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    u.UserName LIKE CONCAT('%', @p{paramIndex}, '%') OR
                    u.FullName LIKE CONCAT('%', @p{paramIndex}, '%')
                )");
                parameters.Add(searchValue);
                paramIndex++;
            }

            var result = await LoadDataTableAsync(sql.ToString(), parameters.ToArray());
            
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

    public async Task<Result<List<string>>> GetUniqueTableNamesAsync()
    {
        try
        {
            var sql = "SELECT DISTINCT TableName FROM Logs ORDER BY TableName";
            var result = await LoadDataTableAsync(sql);
            
            if (result.IsSuccess && result.Data != null)
            {
                var tableNames = new List<string>();
                foreach (System.Data.DataRow row in result.Data.Rows)
                {
                    var tableName = row["TableName"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(tableName))
                    {
                        tableNames.Add(tableName);
                    }
                }
                return Result<List<string>>.Success(tableNames);
            }
            
            return Result<List<string>>.Failure("Unable to get table names");
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Failure($"Error getting table names: {ex.Message}");
        }
    }
}