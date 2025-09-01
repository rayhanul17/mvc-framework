using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace MRCMS.Core.Models.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public int VersionNumber { get; set; } = 1; // Version tracking for audit
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}