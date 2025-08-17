using Microsoft.AspNetCore.Http;
using Nexora.Core.Common;

namespace Nexora.Application.Interfaces;

public interface IBlogImportService
{
    Task<Result<BlogImportData>> ParseExcelFileAsync(IFormFile file);
    Task<Result<BlogImportData>> ValidateImportDataAsync(BlogImportData data, BlogImportOptions options);
    Task<Result<BlogImportProcessResult>> ProcessImportAsync(BlogImportData data, BlogImportOptions options);
    Task<Result<string>> SaveTempFileAsync(IFormFile file);
    Task<Result<BlogImportData>> LoadTempFileAsync(string tempFileName);
    void DeleteTempFile(string tempFileName);
}

public class BlogImportData
{
    public List<BlogImportRowData> Rows { get; set; } = new();
    public List<string> Headers { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
}

public class BlogImportRowData
{
    public int RowNumber { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
    public bool IsValid { get; set; } = true;
    public List<string> ValidationErrors { get; set; } = new();
    
    // Parsed properties
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Author { get; set; }
    public DateTime? PublishDate { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? FeaturedImage { get; set; }
    public int ViewCount { get; set; }
}

public class BlogImportOptions
{
    public bool UpdateExisting { get; set; }
    public bool CreateCategories { get; set; } = true;
    public bool CreateTags { get; set; } = true;
    public bool SkipInvalidRows { get; set; } = true;
    public string ImportMode { get; set; } = "CreateNew"; // CreateNew, UpdateExisting, CreateOrUpdate
}

public class BlogImportProcessResult
{
    public int SuccessCount { get; set; }
    public int UpdatedCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<BlogImportError> Errors { get; set; } = new();
    public List<int> ImportedPostIds { get; set; } = new();
    public List<int> UpdatedPostIds { get; set; } = new();
}

public class BlogImportError
{
    public int RowNumber { get; set; }
    public string Error { get; set; } = string.Empty;
    public Dictionary<string, object?> RowData { get; set; } = new();
}