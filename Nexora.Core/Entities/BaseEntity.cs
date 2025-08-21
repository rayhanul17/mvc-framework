namespace Nexora.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }  // User ID who created the record
    public string? ModifiedBy { get; set; } // User ID who last modified the record
    public int VersionNumber { get; set; } = 1; // Tracks the number of updates, starts at 1
}