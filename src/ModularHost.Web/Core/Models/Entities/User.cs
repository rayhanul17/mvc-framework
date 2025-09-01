using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRCMS.Core.Models.Entities
{
    public class User : IdentityUser<Guid>
    {
        // Custom fields
        public string FullName { get; set; } = "";
        public string? Avatar { get; set; } // URL or path to avatar image
        public string? Description { get; set; } // User bio/description
        public string? ProfilePicture { get; set; } // Keep for backward compatibility, will migrate to Avatar
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
        public int VersionNumber { get; set; } = 1; // Version tracking for audit
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        
        // Helper properties
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : UserName ?? Email ?? "User";
        public string AvatarUrl => !string.IsNullOrEmpty(Avatar) ? Avatar : (!string.IsNullOrEmpty(ProfilePicture) ? ProfilePicture : "/images/default-avatar.png");
        
        // Backward compatibility - will be removed after migration
        public string FirstName 
        { 
            get => FullName?.Split(' ').FirstOrDefault() ?? "";
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var parts = FullName?.Split(' ') ?? new string[] { "", "" };
                    FullName = value + (parts.Length > 1 ? " " + string.Join(" ", parts.Skip(1)) : "");
                }
            }
        }
        
        public string LastName 
        { 
            get => FullName?.Contains(' ') == true ? FullName.Substring(FullName.IndexOf(' ') + 1) : "";
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var firstName = FullName?.Split(' ').FirstOrDefault() ?? "";
                    FullName = (string.IsNullOrEmpty(firstName) ? "" : firstName + " ") + value;
                }
            }
        }
    }
}