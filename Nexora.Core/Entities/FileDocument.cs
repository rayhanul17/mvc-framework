using Nexora.Core.Common;

namespace Nexora.Core.Entities;

public class FileDocument : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public int DownloadCount { get; set; } = 0;
    public DateTime? LastDownloadedAt { get; set; }
    public string? UploadedBy { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
}