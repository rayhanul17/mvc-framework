using Microsoft.AspNetCore.Identity;

namespace Nexora.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; } = false;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    // Helper property to get display name (nickname or username)
    public string DisplayName => !string.IsNullOrWhiteSpace(Nickname) ? Nickname : UserName ?? "User";
}