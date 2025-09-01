using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MRCMS.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MRCMS.Core.Infrastructure
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly List<AuditEntry> _auditEntries = new();

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context != null)
            {
                OnBeforeSaveChanges(eventData.Context);
            }
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                OnBeforeSaveChanges(eventData.Context);
            }
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            if (eventData.Context != null)
            {
                _ = OnAfterSaveChangesAsync(eventData.Context);
            }
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                await OnAfterSaveChangesAsync(eventData.Context);
            }
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void OnBeforeSaveChanges(DbContext context)
        {
            _auditEntries.Clear();
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = httpContext?.User?.Identity?.Name;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    UserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                    UserName = userName,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _auditEntries.Add(auditEntry);

                // Update version number and audit fields for BaseEntity
                if (entry.Entity is BaseEntity baseEntity)
                {
                    var now = DateTime.UtcNow;
                    var userGuid = string.IsNullOrEmpty(userId) ? (Guid?)null : Guid.Parse(userId);

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.CreatedAt = now;
                            baseEntity.CreatedBy = userGuid;
                            baseEntity.VersionNumber = 1;
                            break;

                        case EntityState.Modified:
                            baseEntity.UpdatedAt = now;
                            baseEntity.UpdatedBy = userGuid;
                            baseEntity.VersionNumber++;
                            auditEntry.VersionNumber = baseEntity.VersionNumber;
                            break;
                    }
                }

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    
                    // Skip audit fields
                    if (propertyName == "CreatedAt" || propertyName == "CreatedBy" || 
                        propertyName == "UpdatedAt" || propertyName == "UpdatedBy")
                        continue;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                auditEntry.ChangedProperties.Add(propertyName);
                            }
                            break;
                    }
                }
            }
        }

        private async Task OnAfterSaveChangesAsync(DbContext context)
        {
            if (_auditEntries == null || !_auditEntries.Any())
                return;

            var auditLogs = new List<AuditLog>();

            foreach (var auditEntry in _auditEntries)
            {
                // Update temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                var auditLog = new AuditLog
                {
                    EntityName = auditEntry.EntityName,
                    EntityId = GetEntityId(auditEntry.KeyValues),
                    Action = auditEntry.Action,
                    VersionNumber = auditEntry.VersionNumber,
                    OldValues = auditEntry.OldValues.Count == 0 ? null : JsonSerializer.Serialize(auditEntry.OldValues),
                    NewValues = auditEntry.NewValues.Count == 0 ? null : JsonSerializer.Serialize(auditEntry.NewValues),
                    ChangedProperties = auditEntry.ChangedProperties.Count == 0 ? null : string.Join(",", auditEntry.ChangedProperties),
                    UserId = auditEntry.UserId,
                    UserName = auditEntry.UserName,
                    IpAddress = auditEntry.IpAddress,
                    UserAgent = auditEntry.UserAgent,
                    Timestamp = DateTime.UtcNow
                };

                auditLogs.Add(auditLog);
            }

            if (auditLogs.Any())
            {
                context.Set<AuditLog>().AddRange(auditLogs);
                await context.SaveChangesAsync();
            }
        }

        private Guid GetEntityId(Dictionary<string, object?> keyValues)
        {
            if (keyValues.TryGetValue("Id", out var id) && id is Guid guidId)
            {
                return guidId;
            }
            return Guid.Empty;
        }
    }

    internal class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
            EntityName = entry.Entity.GetType().Name;
            Action = entry.State.ToString();
        }

        public EntityEntry Entry { get; }
        public string EntityName { get; }
        public string Action { get; }
        public int VersionNumber { get; set; } = 1;
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<string> ChangedProperties { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
    }
}