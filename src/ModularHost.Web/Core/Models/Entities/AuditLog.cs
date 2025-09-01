using System;
using System.ComponentModel.DataAnnotations;

namespace MRCMS.Core.Models.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = "";
        
        public Guid EntityId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = ""; // Create, Update, Delete
        
        public int VersionNumber { get; set; }
        
        public string? OldValues { get; set; } // JSON of old values
        
        public string? NewValues { get; set; } // JSON of new values
        
        public string? ChangedProperties { get; set; } // Comma-separated list of changed properties
        
        public Guid? UserId { get; set; }
        
        [MaxLength(100)]
        public string? UserName { get; set; }
        
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        
        [MaxLength(255)]
        public string? UserAgent { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string? Comments { get; set; }
        
        // Navigation property
        public virtual User? User { get; set; }
    }
}