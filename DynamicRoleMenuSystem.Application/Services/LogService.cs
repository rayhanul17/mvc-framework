using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Core.Interfaces;

namespace DynamicRoleMenuSystem.Application.Services;

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
    
    public async Task<Result<Log>> LogCreateAsync<T>(T entity) where T : BaseEntity
    {
        try
        {
            if (!_auditLogSettings.Enabled)
                return Result<Log>.Success(null!);
                
            var tableName = typeof(T).Name;
            var newValues = JsonSerializer.Serialize(entity);
            
            return await LogAsync(tableName, entity.Id, "Create", null, newValues, $"Created new {tableName}");
        }
        catch (Exception ex)
        {
            return Result<Log>.Failure($"Failed to log create: {ex.Message}");
        }
    }
    
    public async Task<Result<Log>> LogUpdateAsync<T>(T oldEntity, T newEntity) where T : BaseEntity
    {
        try
        {
            if (!_auditLogSettings.Enabled)
                return Result<Log>.Success(null!);
                
            var tableName = typeof(T).Name;
            var oldValues = JsonSerializer.Serialize(oldEntity);
            var newValues = JsonSerializer.Serialize(newEntity);
            var changes = GetChanges(oldEntity, newEntity);
            
            return await LogAsync(tableName, newEntity.Id, "Update", oldValues, newValues, changes);
        }
        catch (Exception ex)
        {
            return Result<Log>.Failure($"Failed to log update: {ex.Message}");
        }
    }
    
    public async Task<Result<Log>> LogDeleteAsync<T>(T entity) where T : BaseEntity
    {
        try
        {
            if (!_auditLogSettings.Enabled)
                return Result<Log>.Success(null!);
                
            var tableName = typeof(T).Name;
            var oldValues = JsonSerializer.Serialize(entity);
            
            return await LogAsync(tableName, entity.Id, "Delete", oldValues, null, $"Deleted {tableName}");
        }
        catch (Exception ex)
        {
            return Result<Log>.Failure($"Failed to log delete: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<Log>>> GetLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null)
    {
        try
        {
            var logs = await _logRepository.GetLogsAsync(
                tableName, entityId, userId, startDate, endDate, action, skip, take);
            return Result<IEnumerable<Log>>.Success(logs);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Log>>.Failure($"Failed to get logs: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<LogArchive>>> GetArchivedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null)
    {
        try
        {
            var logs = await _logRepository.GetArchivedLogsAsync(
                tableName, entityId, userId, startDate, endDate, action, skip, take);
            return Result<IEnumerable<LogArchive>>.Success(logs);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<LogArchive>>.Failure($"Failed to get archived logs: {ex.Message}");
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
        int? take = null)
    {
        try
        {
            if (source == "log")
            {
                var logs = await _logRepository.GetLogsAsync(
                    tableName, entityId, userId, startDate, endDate, action, skip, take);
                return Result<IEnumerable<object>>.Success(logs.Cast<object>());
            }
            else if (source == "archive")
            {
                var archives = await _logRepository.GetArchivedLogsAsync(
                    tableName, entityId, userId, startDate, endDate, action, skip, take);
                return Result<IEnumerable<object>>.Success(archives.Cast<object>());
            }
            else
            {
                var combined = await _logRepository.GetCombinedLogsAsync(
                    tableName, entityId, userId, startDate, endDate, action, true, skip, take);
                return Result<IEnumerable<object>>.Success(combined);
            }
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<object>>.Failure($"Failed to get combined logs: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<string>>> GetTableNamesAsync()
    {
        try
        {
            var tableNames = await _logRepository.GetDistinctTableNamesAsync();
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
    
    private string GetChanges<T>(T oldEntity, T newEntity) where T : BaseEntity
    {
        var changes = new List<string>();
        var properties = typeof(T).GetProperties();
        
        foreach (var property in properties)
        {
            if (property.Name == "UpdatedAt" || property.Name == "ModifiedBy")
                continue;
                
            var oldValue = property.GetValue(oldEntity);
            var newValue = property.GetValue(newEntity);
            
            if (!Equals(oldValue, newValue))
            {
                changes.Add($"{property.Name}: {oldValue} -> {newValue}");
            }
        }
        
        return string.Join(", ", changes);
    }
}