using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ModularHost.Web.Core.Models.Entities
{
    public class User : IdentityUser<Guid>
    {
        // Custom fields
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? ProfilePicture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        
        // Audit fields (not inheriting from BaseEntity since IdentityUser already has Id)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; } // Who created this user
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; } // Who last updated this user
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
        public string FullName => $"{FirstName} {LastName}";
    }
}