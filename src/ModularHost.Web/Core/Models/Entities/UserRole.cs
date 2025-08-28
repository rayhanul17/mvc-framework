using Microsoft.AspNetCore.Identity;
using System;

namespace ModularHost.Web.Core.Models.Entities
{
    public class UserRole : IdentityUserRole<Guid>
    {
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}