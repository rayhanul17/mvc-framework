using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class LogService : ILogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogRepository _logRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuditLogSettings _auditLogSettings;
    
    public LogService(
        IUnitOfWork unitOfWork, 
        ILogRepository logRepository,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AppSettings> appSettings)
    {
        _unitOfWork = unitOfWork;
        _logRepository = logRepository;
        _httpContextAccessor = httpContextAccessor;
        _auditLogSettings = appSettings.Value.AuditLog;
    }
    
    public async Task<Result<Log>> LogAsync(string tableName, int entityId, string action, 
        string? oldValues = null, string? newValues = null, string? changes = null)
    {
        try
        {
            if (!_auditLogSettings.Enabled)
                return Result<Log>.Success(null!);
                
            var httpContext = _httpContextAccessor.HttpContext;
            var log = new Log
            {
                TableName = tableName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                Changes = changes,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                LoggedAt = DateTime.UtcNow,
                UserId = httpContext?.User?.Identity?.Name
            };
            
            await _logRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<Log>.Success(log);
        }
        catch (Exception ex)
        {
            return Result<Log>.Failure($"Failed to create log: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<object>>> GetCombinedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        string source = "all",
        int? skip = null,
        int? take = null,
        string? ipAddress = null,
        string? keyword = null)
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var connectionString = context.Database.GetConnectionString();
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            var sql = new StringBuilder();
            var parameters = new List<MySqlParameter>();
            var paramIndex = 0;
            
            if (source == "log" || source == "all")
            {
                sql.Append(@"
                    SELECT 
                        l.Id,
                        l.TableName,
                        l.EntityId,
                        l.Action,
                        l.OldValues,
                        l.NewValues,
                        l.Changes,
                        l.IpAddress,
                        l.UserAgent,
                        l.LoggedAt,
                        l.UserId,
                        u.UserName,
                        u.FullName,
                        0 as IsArchived,
                        NULL as ArchivedAt
                    FROM Logs l
                    LEFT JOIN Users u ON l.UserId = u.Id
                    WHERE 1=1
                ");
                
                AddWhereConditions(sql, parameters, ref paramIndex, tableName, entityId, userId, startDate, endDate, action, ipAddress, keyword);
            }
            
            if (source == "all")
            {
                sql.Append(" UNION ALL ");
            }
            
            if (source == "archive" || source == "all")
            {
                sql.Append(@"
                    SELECT 
                        a.Id,
                        a.TableName,
                        a.EntityId,
                        a.Action,
                        a.OldValues,
                        a.NewValues,
                        a.Changes,
                        a.IpAddress,
                        a.UserAgent,
                        a.LoggedAt,
                        a.UserId,
                        u.UserName,
                        u.FullName,
                        1 as IsArchived,
                        a.ArchivedAt
                    FROM LogArchives a
                    LEFT JOIN Users u ON a.UserId = u.Id
                    WHERE 1=1
                ");
                
                AddWhereConditions(sql, parameters, ref paramIndex, tableName, entityId, userId, startDate, endDate, action, ipAddress, keyword);
            }
            
            sql.Append(" ORDER BY LoggedAt DESC");
            
            if (take.HasValue)
            {
                sql.Append($" LIMIT {take.Value}");
                if (skip.HasValue)
                {
                    sql.Append($" OFFSET {skip.Value}");
                }
            }
            
            using var command = new MySqlCommand(sql.ToString(), connection);
            command.Parameters.AddRange(parameters.ToArray());
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<object>();
            
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    Id = reader.GetInt32("Id"),
                    TableName = reader.GetString("TableName"),
                    EntityId = reader.GetInt32("EntityId"),
                    Action = reader.GetString("Action"),
                    OldValues = reader.IsDBNull(reader.GetOrdinal("OldValues")) ? null : reader.GetString("OldValues"),
                    NewValues = reader.IsDBNull(reader.GetOrdinal("NewValues")) ? null : reader.GetString("NewValues"),
                    Changes = reader.IsDBNull(reader.GetOrdinal("Changes")) ? null : reader.GetString("Changes"),
                    IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString("IpAddress"),
                    UserAgent = reader.IsDBNull(reader.GetOrdinal("UserAgent")) ? null : reader.GetString("UserAgent"),
                    LoggedAt = reader.GetDateTime("LoggedAt"),
                    UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString("UserId"),
                    User = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : new
                    {
                        UserName = reader.GetString("UserName"),
                        FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString("FullName")
                    },
                    IsArchived = reader.GetBoolean("IsArchived"),
                    ArchivedAt = reader.IsDBNull(reader.GetOrdinal("ArchivedAt")) ? (DateTime?)null : reader.GetDateTime("ArchivedAt")
                });
            }
            
            return Result<IEnumerable<object>>.Success(results);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<object>>.Failure($"Failed to get combined logs: {ex.Message}");
        }
    }
    
    private void AddWhereConditions(StringBuilder sql, List<MySqlParameter> parameters, ref int paramIndex,
        string? tableName, int? entityId, string? userId, DateTime? startDate, DateTime? endDate, string? action,
        string? ipAddress, string? keyword)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            sql.Append($" AND TableName = @p{paramIndex}");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", tableName));
            paramIndex++;
        }
        
        if (entityId.HasValue)
        {
            sql.Append($" AND EntityId = @p{paramIndex}");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", entityId.Value));
            paramIndex++;
        }
        
        if (!string.IsNullOrWhiteSpace(userId))
        {
            // Search by username instead of userId
            sql.Append($" AND (u.UserName LIKE @p{paramIndex} OR u.FullName LIKE @p{paramIndex})");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", $"%{userId}%"));
            paramIndex++;
        }
        
        if (startDate.HasValue)
        {
            sql.Append($" AND LoggedAt >= @p{paramIndex}");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", startDate.Value));
            paramIndex++;
        }
        
        if (endDate.HasValue)
        {
            // Add time to end date to include the entire day
            var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
            sql.Append($" AND LoggedAt <= @p{paramIndex}");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", endDateTime));
            paramIndex++;
        }
        
        if (!string.IsNullOrWhiteSpace(action))
        {
            sql.Append($" AND Action = @p{paramIndex}");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", action));
            paramIndex++;
        }
        
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            sql.Append($" AND IpAddress LIKE @p{paramIndex}");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", $"%{ipAddress}%"));
            paramIndex++;
        }
        
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql.Append($" AND (Changes LIKE @p{paramIndex} OR OldValues LIKE @p{paramIndex} OR NewValues LIKE @p{paramIndex})");
            parameters.Add(new MySqlParameter($"@p{paramIndex}", $"%{keyword}%"));
            paramIndex++;
        }
    }
    
    public async Task<Result<Log>> GetLogByIdAsync(int id, string source = "log")
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var connectionString = context.Database.GetConnectionString();
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            string tableName = source switch
            {
                "archive" => "LogArchives",
                _ => "Logs"
            };
            
            var sql = $@"
                SELECT l.*, u.UserName, u.FullName 
                FROM {tableName} l
                LEFT JOIN Users u ON l.UserId = u.Id
                WHERE l.Id = @id";
            
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var log = new Log
                {
                    Id = reader.GetInt32("Id"),
                    TableName = reader.GetString("TableName"),
                    EntityId = reader.GetInt32("EntityId"),
                    Action = reader.GetString("Action"),
                    OldValues = reader.IsDBNull(reader.GetOrdinal("OldValues")) ? null : reader.GetString("OldValues"),
                    NewValues = reader.IsDBNull(reader.GetOrdinal("NewValues")) ? null : reader.GetString("NewValues"),
                    Changes = reader.IsDBNull(reader.GetOrdinal("Changes")) ? null : reader.GetString("Changes"),
                    IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString("IpAddress"),
                    UserAgent = reader.IsDBNull(reader.GetOrdinal("UserAgent")) ? null : reader.GetString("UserAgent"),
                    LoggedAt = reader.GetDateTime("LoggedAt"),
                    UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString("UserId")
                };
                
                // Add user information if available
                if (!reader.IsDBNull(reader.GetOrdinal("UserName")))
                {
                    log.User = new ApplicationUser
                    {
                        UserName = reader.GetString("UserName"),
                        FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString("FullName")
                    };
                }
                
                return Result<Log>.Success(log);
            }
            
            return Result<Log>.Failure($"Log with ID {id} not found in {tableName}");
        }
        catch (Exception ex)
        {
            return Result<Log>.Failure($"Failed to get log by ID: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<string>>> GetTableNamesAsync()
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var connectionString = context.Database.GetConnectionString();
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            var sql = @"
                SELECT DISTINCT TableName FROM (
                    SELECT DISTINCT TableName FROM Logs
                    UNION
                    SELECT DISTINCT TableName FROM LogArchives
                ) AS combined
                ORDER BY TableName
            ";
            
            using var command = new MySqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            var tableNames = new List<string>();
            while (await reader.ReadAsync())
            {
                tableNames.Add(reader.GetString("TableName"));
            }
            
            return Result<IEnumerable<string>>.Success(tableNames);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<string>>.Failure($"Failed to get table names: {ex.Message}");
        }
    }
    
    public async Task<Result> ArchiveOldLogsAsync()
    {
        try
        {
            var archiveOlderThan = DateTime.UtcNow.Subtract(_auditLogSettings.ArchiveSettings.GetTimeSpan());
            await _logRepository.ArchiveLogsAsync(archiveOlderThan);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to archive logs: {ex.Message}");
        }
    }
    
    public async Task<Result> CleanupArchivedLogsAsync()
    {
        try
        {
            var deleteOlderThan = DateTime.UtcNow.AddDays(-_auditLogSettings.RetentionDays);
            await _logRepository.DeleteOldArchivedLogsAsync(deleteOlderThan);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to cleanup archived logs: {ex.Message}");
        }
    }
}