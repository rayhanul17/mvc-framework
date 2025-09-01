using System;

namespace MRCMS.Core.Models.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; } // Nullable for anonymous actions
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; } // Nullable for anonymous actions
        public int VersionNumber { get; set; } = 1; // Tracks total modification count
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}