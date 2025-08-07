using Microsoft.AspNetCore.Identity;

namespace DynamicRoleMenuSystem.Core.Entities;

public class UserRole : IdentityUserRole<string>
{
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}