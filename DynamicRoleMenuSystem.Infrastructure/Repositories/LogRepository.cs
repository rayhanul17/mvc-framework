using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using DynamicRoleMenuSystem.Core.Entities;
using DynamicRoleMenuSystem.Core.Interfaces;
using DynamicRoleMenuSystem.Infrastructure.Data;

namespace DynamicRoleMenuSystem.Infrastructure.Repositories;

public class LogRepository : BaseRepository<Log>, ILogRepository
{
    private new readonly ApplicationDbContext _context;
    
    public LogRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Log>> GetLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null)
    {
        var query = _context.Logs.Include(l => l.User).AsQueryable();
        
        if (!string.IsNullOrEmpty(tableName))
            query = query.Where(l => l.TableName == tableName);
            
        if (entityId.HasValue)
            query = query.Where(l => l.EntityId == entityId.Value);
            
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);
            
        if (startDate.HasValue)
            query = query.Where(l => l.LoggedAt >= startDate.Value);
            
        if (endDate.HasValue)
            query = query.Where(l => l.LoggedAt <= endDate.Value);
            
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
            
        query = query.OrderByDescending(l => l.LoggedAt);
        
        if (skip.HasValue)
            query = query.Skip(skip.Value);
            
        if (take.HasValue)
            query = query.Take(take.Value);
            
        return await query.ToListAsync();
    }
    
    public async Task<IEnumerable<LogArchive>> GetArchivedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        int? skip = null,
        int? take = null)
    {
        var query = _context.LogArchives.Include(l => l.User).AsQueryable();
        
        if (!string.IsNullOrEmpty(tableName))
            query = query.Where(l => l.TableName == tableName);
            
        if (entityId.HasValue)
            query = query.Where(l => l.EntityId == entityId.Value);
            
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);
            
        if (startDate.HasValue)
            query = query.Where(l => l.LoggedAt >= startDate.Value);
            
        if (endDate.HasValue)
            query = query.Where(l => l.LoggedAt <= endDate.Value);
            
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
            
        query = query.OrderByDescending(l => l.LoggedAt);
        
        if (skip.HasValue)
            query = query.Skip(skip.Value);
            
        if (take.HasValue)
            query = query.Take(take.Value);
            
        return await query.ToListAsync();
    }
    
    public async Task<IEnumerable<object>> GetCombinedLogsAsync(
        string? tableName = null,
        int? entityId = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? action = null,
        bool includeArchived = false,
        int? skip = null,
        int? take = null)
    {
        var logsQuery = _context.Logs.Include(l => l.User)
            .Select(l => new
            {
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
                User = l.User,
                IsArchived = false,
                ArchivedAt = (DateTime?)null
            });
            
        var combinedQuery = logsQuery.AsQueryable();
        
        if (includeArchived)
        {
            var archivesQuery = _context.LogArchives.Include(l => l.User)
                .Select(l => new
                {
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
                    User = l.User,
                    IsArchived = true,
                    ArchivedAt = (DateTime?)l.ArchivedAt
                });
                
            combinedQuery = logsQuery.Union(archivesQuery);
        }
        
        if (!string.IsNullOrEmpty(tableName))
            combinedQuery = combinedQuery.Where(l => l.TableName == tableName);
            
        if (entityId.HasValue)
            combinedQuery = combinedQuery.Where(l => l.EntityId == entityId.Value);
            
        if (!string.IsNullOrEmpty(userId))
            combinedQuery = combinedQuery.Where(l => l.UserId == userId);
            
        if (startDate.HasValue)
            combinedQuery = combinedQuery.Where(l => l.LoggedAt >= startDate.Value);
            
        if (endDate.HasValue)
            combinedQuery = combinedQuery.Where(l => l.LoggedAt <= endDate.Value);
            
        if (!string.IsNullOrEmpty(action))
            combinedQuery = combinedQuery.Where(l => l.Action == action);
            
        combinedQuery = combinedQuery.OrderByDescending(l => l.LoggedAt);
        
        if (skip.HasValue)
            combinedQuery = combinedQuery.Skip(skip.Value);
            
        if (take.HasValue)
            combinedQuery = combinedQuery.Take(take.Value);
            
        return await combinedQuery.ToListAsync();
    }
    
    public async Task<int> GetLogCountAsync(Expression<Func<Log, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _context.Logs.CountAsync();
            
        return await _context.Logs.CountAsync(predicate);
    }
    
    public async Task<int> GetArchivedLogCountAsync(Expression<Func<LogArchive, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _context.LogArchives.CountAsync();
            
        return await _context.LogArchives.CountAsync(predicate);
    }
    
    public async Task<IEnumerable<string>> GetDistinctTableNamesAsync()
    {
        var logTables = await _context.Logs
            .Select(l => l.TableName)
            .Distinct()
            .ToListAsync();
            
        var archiveTables = await _context.LogArchives
            .Select(l => l.TableName)
            .Distinct()
            .ToListAsync();
            
        return logTables.Union(archiveTables).Distinct().OrderBy(t => t);
    }
    
    public async Task ArchiveLogsAsync(DateTime olderThan)
    {
        var logsToArchive = await _context.Logs
            .Where(l => l.LoggedAt < olderThan)
            .ToListAsync();
            
        if (logsToArchive.Any())
        {
            var archives = logsToArchive.Select(log => new LogArchive
            {
                TableName = log.TableName,
                EntityId = log.EntityId,
                Action = log.Action,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                Changes = log.Changes,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                LoggedAt = log.LoggedAt,
                ArchivedAt = DateTime.UtcNow,
                UserId = log.UserId,
                CreatedAt = log.CreatedAt,
                UpdatedAt = log.UpdatedAt,
                CreatedBy = log.CreatedBy,
                ModifiedBy = log.ModifiedBy
            });
            
            await _context.LogArchives.AddRangeAsync(archives);
            _context.Logs.RemoveRange(logsToArchive);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task DeleteOldArchivedLogsAsync(DateTime olderThan)
    {
        var logsToDelete = await _context.LogArchives
            .Where(l => l.ArchivedAt < olderThan)
            .ToListAsync();
            
        if (logsToDelete.Any())
        {
            _context.LogArchives.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();
        }
    }
}