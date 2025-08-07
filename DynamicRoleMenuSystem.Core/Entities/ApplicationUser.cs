using Microsoft.AspNetCore.Identity;

namespace DynamicRoleMenuSystem.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}