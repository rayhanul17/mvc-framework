using Microsoft.EntityFrameworkCore;
using Nexora.Core.Entities;
using Nexora.Infrastructure.Data;
using System.Text.Json;

namespace Nexora.Application.Services;

public interface IPermissionAuditService
{
    Task LogPermissionChangeAsync(string action, string entityType, string entityId, string userId, object? oldValue = null, object? newValue = null);
    Task LogRolePermissionChangeAsync(string roleId, string roleName, List<int> oldPermissions, List<int> newPermissions, string userId);
    Task LogUserRoleChangeAsync(string userId, string userName, List<string> oldRoles, List<string> newRoles, string changedBy);
}

public class PermissionAuditService : IPermissionAuditService
{
    private readonly ApplicationDbContext _context;

    public PermissionAuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogPermissionChangeAsync(string action, string entityType, string entityId, string userId, object? oldValue = null, object? newValue = null)
    {
        var log = new Log
        {
            TableName = entityType,
            EntityId = int.TryParse(entityId, out var id) ? id : 0,
            Action = action,
            UserId = userId,
            OldValues = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
            NewValues = newValue != null ? JsonSerializer.Serialize(newValue) : null,
            Changes = $"Permission {action}: {entityType} - {entityId}",
            EntityVersionNumber = 1,
            IpAddress = "127.0.0.1",
            UserAgent = "PermissionAuditService",
            LoggedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogRolePermissionChangeAsync(string roleId, string roleName, List<int> oldPermissions, List<int> newPermissions, string userId)
    {
        var added = newPermissions.Except(oldPermissions).ToList();
        var removed = oldPermissions.Except(newPermissions).ToList();

        if (!added.Any() && !removed.Any())
            return; // No changes

        var changeDetails = new
        {
            RoleId = roleId,
            RoleName = roleName,
            PermissionsAdded = added.Count,
            PermissionsRemoved = removed.Count,
            AddedPermissionIds = added,
            RemovedPermissionIds = removed,
            TotalPermissions = newPermissions.Count
        };

        var log = new Log
        {
            TableName = "RolePermissions",
            EntityId = 0, // Role permission changes don't have a single entity ID
            Action = "UpdateRolePermissions",
            UserId = userId,
            OldValues = JsonSerializer.Serialize(new { PermissionIds = oldPermissions }),
            NewValues = JsonSerializer.Serialize(new { PermissionIds = newPermissions }),
            Changes = $"Role permissions updated for {roleName}: +{added.Count} -{removed.Count} permissions. Details: {JsonSerializer.Serialize(changeDetails)}",
            EntityVersionNumber = 1,
            IpAddress = "127.0.0.1",
            UserAgent = "PermissionAuditService",
            LoggedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogUserRoleChangeAsync(string userId, string userName, List<string> oldRoles, List<string> newRoles, string changedBy)
    {
        var added = newRoles.Except(oldRoles).ToList();
        var removed = oldRoles.Except(newRoles).ToList();

        if (!added.Any() && !removed.Any())
            return; // No changes

        var changeDetails = new
        {
            UserId = userId,
            UserName = userName,
            RolesAdded = added,
            RolesRemoved = removed,
            TotalRoles = newRoles.Count,
            ChangedBy = changedBy
        };

        var log = new Log
        {
            TableName = "UserRoles",
            EntityId = 0, // User role changes don't have a single entity ID
            Action = "UpdateUserRoles",
            UserId = changedBy,
            OldValues = JsonSerializer.Serialize(new { Roles = oldRoles }),
            NewValues = JsonSerializer.Serialize(new { Roles = newRoles }),
            Changes = $"User roles updated for {userName}: Added [{string.Join(", ", added)}], Removed [{string.Join(", ", removed)}]. Details: {JsonSerializer.Serialize(changeDetails)}",
            EntityVersionNumber = 1,
            IpAddress = "127.0.0.1",
            UserAgent = "PermissionAuditService",
            LoggedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// View model for permission audit logs
/// </summary>
public class PermissionAuditViewModel
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Message { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Dictionary<string, object>? ChangeDetails { get; set; }
}

/// <summary>
/// Service to retrieve and analyze permission audit logs
/// </summary>
public interface IPermissionAuditReportService
{
    Task<List<PermissionAuditViewModel>> GetRecentAuditLogsAsync(int days = 7);
    Task<List<PermissionAuditViewModel>> GetAuditLogsByUserAsync(string userId, int days = 30);
    Task<List<PermissionAuditViewModel>> GetAuditLogsByRoleAsync(string roleId, int days = 30);
    Task<Dictionary<string, int>> GetPermissionChangeStatisticsAsync(int days = 30);
}

public class PermissionAuditReportService : IPermissionAuditReportService
{
    private readonly ApplicationDbContext _context;

    public PermissionAuditReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionAuditViewModel>> GetRecentAuditLogsAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var logs = await _context.Logs
            .Where(l => l.TableName.StartsWith("Role") || l.TableName.StartsWith("Permission") || l.TableName.StartsWith("User"))
            .Where(l => l.LoggedAt >= startDate)
            .OrderByDescending(l => l.LoggedAt)
            .Select(l => new PermissionAuditViewModel
            {
                Id = l.Id,
                Timestamp = l.LoggedAt,
                Action = l.Action ?? "",
                EntityType = l.TableName ?? "",
                EntityId = l.EntityId.ToString(),
                UserId = l.UserId ?? "",
                Message = l.Changes ?? "",
                OldValues = l.OldValues,
                NewValues = l.NewValues
            })
            .ToListAsync();

        // Populate user names
        var userIds = logs.Select(l => l.UserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();

        foreach (var log in logs)
        {
            log.UserName = users.FirstOrDefault(u => u.Id == log.UserId)?.FullName ?? "Unknown";
            
            // Parse change details if available
            if (!string.IsNullOrEmpty(log.NewValues))
            {
                try
                {
                    log.ChangeDetails = JsonSerializer.Deserialize<Dictionary<string, object>>(log.NewValues);
                }
                catch { }
            }
        }

        return logs;
    }

    public async Task<List<PermissionAuditViewModel>> GetAuditLogsByUserAsync(string userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        return await _context.Logs
            .Where(l => (l.TableName.StartsWith("Role") || l.TableName.StartsWith("Permission") || l.TableName.StartsWith("User")) && 
                       l.UserId == userId && 
                       l.LoggedAt >= startDate)
            .OrderByDescending(l => l.LoggedAt)
            .Select(l => new PermissionAuditViewModel
            {
                Id = l.Id,
                Timestamp = l.LoggedAt,
                Action = l.Action ?? "",
                EntityType = l.TableName ?? "",
                EntityId = l.EntityId.ToString(),
                UserId = l.UserId ?? "",
                Message = l.Changes ?? "",
                OldValues = l.OldValues,
                NewValues = l.NewValues
            })
            .ToListAsync();
    }

    public async Task<List<PermissionAuditViewModel>> GetAuditLogsByRoleAsync(string roleId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        return await _context.Logs
            .Where(l => l.TableName == "RolePermissions" && 
                       l.LoggedAt >= startDate)
            .OrderByDescending(l => l.LoggedAt)
            .Select(l => new PermissionAuditViewModel
            {
                Id = l.Id,
                Timestamp = l.LoggedAt,
                Action = l.Action ?? "",
                EntityType = l.TableName ?? "",
                EntityId = l.EntityId.ToString(),
                UserId = l.UserId ?? "",
                Message = l.Changes ?? "",
                OldValues = l.OldValues,
                NewValues = l.NewValues
            })
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetPermissionChangeStatisticsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var stats = await _context.Logs
            .Where(l => (l.TableName.StartsWith("Role") || l.TableName.StartsWith("Permission") || l.TableName.StartsWith("User")) && 
                       l.LoggedAt >= startDate)
            .GroupBy(l => l.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(s => s.Action ?? "Unknown", s => s.Count);
    }
}