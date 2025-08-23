using Microsoft.AspNetCore.Identity;

namespace Nexora.Core.Entities;

public class UserRole : IdentityUserRole<string>
{
    public DateTime? ExpiresAt { get; set; } // Optional: for temporary roles
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}