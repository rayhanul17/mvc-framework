using System.ComponentModel.DataAnnotations;

namespace Nexora.Web.Models.ViewModels;

public class BlogImportViewModel
{
    [Required(ErrorMessage = "Please select an Excel file to upload")]
    [Display(Name = "Excel File")]
    public IFormFile ExcelFile { get; set; } = null!;
    
    [Display(Name = "Import Mode")]
    public ImportMode Mode { get; set; } = ImportMode.CreateNew;
    
    [Display(Name = "Update Existing Posts")]
    public bool UpdateExisting { get; set; }
    
    [Display(Name = "Create Categories if Not Exist")]
    public bool CreateCategories { get; set; } = true;
    
    [Display(Name = "Create Tags if Not Exist")]
    public bool CreateTags { get; set; } = true;
}

public enum ImportMode
{
    CreateNew,
    UpdateExisting,
    CreateOrUpdate
}

public class BlogImportPreviewViewModel
{
    public List<BlogImportRow> ValidRows { get; set; } = new();
    public List<BlogImportRow> InvalidRows { get; set; } = new();
    public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
    public ImportStatistics Statistics { get; set; } = new();
    public string TempFileName { get; set; } = string.Empty;
    public ImportMode Mode { get; set; }
    public bool UpdateExisting { get; set; }
    public bool CreateCategories { get; set; }
    public bool CreateTags { get; set; }
}

public class BlogImportRow
{
    public int RowNumber { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; } // Comma-separated
    public string? Author { get; set; }
    public DateTime? PublishDate { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsFeatured { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? FeaturedImage { get; set; }
    public int? ViewCount { get; set; }
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}

public class ImportStatistics
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int NewPosts { get; set; }
    public int UpdatedPosts { get; set; }
    public int NewCategories { get; set; }
    public int NewTags { get; set; }
    public int DuplicateSlugs { get; set; }
}

public class BlogImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ImportedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public ImportStatistics Statistics { get; set; } = new();
}