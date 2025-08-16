using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Nexora.Infrastructure.Data;

public class AuditEntry
{
    public string TableName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Changes { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public EntityEntry? EntityEntry { get; set; }
}