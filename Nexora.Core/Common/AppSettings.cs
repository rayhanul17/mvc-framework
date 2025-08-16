namespace Nexora.Core.Common;

public class AppSettings
{
    public string OrganizationName { get; set; } = "Dynamic Role Menu System";
    public AuditLogSettings AuditLog { get; set; } = new();
}

