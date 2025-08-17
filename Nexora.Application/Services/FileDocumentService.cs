using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Nexora.Core.Common;
using Nexora.Core.Interfaces;
using Nexora.Application.Interfaces;
using Nexora.Core.Entities;
using System.Data;

namespace Nexora.Application.Services;

public class FileDocumentService : BaseService<FileDocument>, IFileDocumentService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<FileDocumentService> _logger;
    private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".zip", ".rar", ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mp3" };
    private const long MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB

    public FileDocumentService(
        IUnitOfWork unitOfWork,
        ILogger<FileDocumentService> logger,
        IWebHostEnvironment environment,
        IFileUploadService fileUploadService) : base(unitOfWork)
    {
        _logger = logger;
        _environment = environment;
        _fileUploadService = fileUploadService;
    }

    public async Task<Result<DataTable>> GetFileDocumentsForDataTableAsync(
        int draw, 
        int start, 
        int length, 
        string searchValue,
        int sortColumn,
        string sortDirection,
        string? category = null,
        string? fileExtension = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var context = _unitOfWork.GetDbContext();
            var connectionString = context.Database.GetConnectionString();
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // Build WHERE clause
            var whereConditions = new List<string> { "IsActive = 1" };
            var parameters = new List<MySqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                whereConditions.Add("(OriginalFileName LIKE @search OR Description LIKE @search OR Tags LIKE @search)");
                parameters.Add(new MySqlParameter("@search", $"%{searchValue}%"));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                whereConditions.Add("Category = @category");
                parameters.Add(new MySqlParameter("@category", category));
            }

            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                whereConditions.Add("FileExtension = @extension");
                parameters.Add(new MySqlParameter("@extension", fileExtension));
            }

            if (startDate.HasValue)
            {
                whereConditions.Add("CreatedAt >= @startDate");
                parameters.Add(new MySqlParameter("@startDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                whereConditions.Add("CreatedAt <= @endDate");
                parameters.Add(new MySqlParameter("@endDate", endDate.Value.AddDays(1).AddSeconds(-1)));
            }

            var whereClause = whereConditions.Count > 0 ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

            // Get total count
            var countQuery = "SELECT COUNT(*) FROM FileDocuments";
            using var countCmd = new MySqlCommand(countQuery, connection);
            var totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync() ?? 0);

            // Get filtered count
            var filteredCountQuery = $"SELECT COUNT(*) FROM FileDocuments {whereClause}";
            using var filteredCountCmd = new MySqlCommand(filteredCountQuery, connection);
            filteredCountCmd.Parameters.AddRange(parameters.ToArray());
            var filteredRecords = Convert.ToInt32(await filteredCountCmd.ExecuteScalarAsync() ?? 0);

            // Define column names for sorting
            var columns = new[] { "Id", "OriginalFileName", "Category", "FileSize", "DownloadCount", "CreatedAt", "UploadedBy" };
            var orderByColumn = sortColumn < columns.Length ? columns[sortColumn] : "CreatedAt";
            var orderByDirection = sortDirection?.ToUpper() == "ASC" ? "ASC" : "DESC";

            // Get paginated data
            var dataQuery = $@"
                SELECT 
                    Id,
                    FileName,
                    OriginalFileName,
                    FilePath,
                    FileExtension,
                    FileSize,
                    ContentType,
                    Category,
                    Description,
                    Tags,
                    DownloadCount,
                    LastDownloadedAt,
                    UploadedBy,
                    IsPublic,
                    CreatedAt,
                    UpdatedAt
                FROM FileDocuments
                {whereClause}
                ORDER BY {orderByColumn} {orderByDirection}
                LIMIT @length OFFSET @start";

            using var dataCmd = new MySqlCommand(dataQuery, connection);
            dataCmd.Parameters.AddRange(parameters.ToArray());
            dataCmd.Parameters.Add(new MySqlParameter("@start", start));
            dataCmd.Parameters.Add(new MySqlParameter("@length", length));

            using var adapter = new MySqlDataAdapter(dataCmd);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            // Add DataTables metadata
            dataTable.ExtendedProperties["draw"] = draw;
            dataTable.ExtendedProperties["recordsTotal"] = totalRecords;
            dataTable.ExtendedProperties["recordsFiltered"] = filteredRecords;

            return Result<DataTable>.Success(dataTable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file documents for DataTable");
            return Result<DataTable>.Failure($"Error loading file documents: {ex.Message}");
        }
    }

    public async Task<Result<FileDocument>> UploadFileAsync(IFormFile file, string category, string? description, string? tags, string uploadedBy)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Result<FileDocument>.Failure("No file provided");

            if (file.Length > MAX_FILE_SIZE)
                return Result<FileDocument>.Failure($"File size exceeds maximum allowed size of {MAX_FILE_SIZE / (1024 * 1024)} MB");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                return Result<FileDocument>.Failure($"File type {extension} is not allowed");

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderName = Path.Combine("uploads", "documents", category.ToLower());
            var fullPath = Path.Combine(_environment.WebRootPath, folderName);

            // Ensure directory exists
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            var filePath = Path.Combine(fullPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create FileDocument entity
            var fileDocument = new FileDocument
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                FilePath = $"/{folderName.Replace('\\', '/')}/{fileName}",
                FileExtension = extension,
                FileSize = file.Length,
                ContentType = file.ContentType ?? "application/octet-stream",
                Category = category,
                Description = description,
                Tags = tags,
                UploadedBy = uploadedBy,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await CreateAsync(fileDocument);
            if (result.IsSuccess)
                return Result<FileDocument>.Success(result.Data);

            // If database save fails, delete the uploaded file
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Result<FileDocument>.Failure("Failed to save file information to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return Result<FileDocument>.Failure($"Error uploading file: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteFileWithPhysicalRemovalAsync(int id)
    {
        try
        {
            var fileResult = await GetByIdAsync(id);
            if (!fileResult.IsSuccess || fileResult.Data == null)
                return Result<bool>.Failure("File not found");

            var file = fileResult.Data;

            // Delete physical file
            if (!string.IsNullOrEmpty(file.FilePath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, file.FilePath.TrimStart('/').Replace('/', '\\'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

            // Delete database record
            var deleteResult = await DeleteAsync(id);
            return deleteResult.IsSuccess 
                ? Result<bool>.Success(true) 
                : Result<bool>.Failure(deleteResult.ErrorMessage ?? "Failed to delete file record");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return Result<bool>.Failure($"Error deleting file: {ex.Message}");
        }
    }

    public async Task<Result<FileDocument>> IncrementDownloadCountAsync(int id)
    {
        try
        {
            var sql = @"
                UPDATE FileDocuments 
                SET DownloadCount = DownloadCount + 1,
                    LastDownloadedAt = @now
                WHERE Id = @id";

            var parameters = new object[] { DateTime.UtcNow, id };
            await ExecuteRawSqlAsync(sql, parameters);

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing download count");
            return Result<FileDocument>.Failure($"Error updating download count: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<string>>> GetCategoriesAsync()
    {
        try
        {
            var sql = "SELECT DISTINCT Category FROM FileDocuments WHERE IsActive = 1 AND Category IS NOT NULL ORDER BY Category";
            var dataTable = await LoadDataTableAsync(sql);
            
            if (!dataTable.IsSuccess)
                return Result<IEnumerable<string>>.Failure(dataTable.ErrorMessage ?? "Failed to load data");

            var categories = new List<string>();
            foreach (DataRow row in dataTable.Data.Rows)
            {
                categories.Add(row["Category"].ToString() ?? string.Empty);
            }

            return Result<IEnumerable<string>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return Result<IEnumerable<string>>.Failure($"Error loading categories: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<string>>> GetFileExtensionsAsync()
    {
        try
        {
            var sql = "SELECT DISTINCT FileExtension FROM FileDocuments WHERE IsActive = 1 ORDER BY FileExtension";
            var dataTable = await LoadDataTableAsync(sql);
            
            if (!dataTable.IsSuccess)
                return Result<IEnumerable<string>>.Failure(dataTable.ErrorMessage ?? "Failed to load data");

            var extensions = new List<string>();
            foreach (DataRow row in dataTable.Data.Rows)
            {
                extensions.Add(row["FileExtension"].ToString() ?? string.Empty);
            }

            return Result<IEnumerable<string>>.Success(extensions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file extensions");
            return Result<IEnumerable<string>>.Failure($"Error loading file extensions: {ex.Message}");
        }
    }

    public async Task<Result<long>> GetTotalFileSizeAsync()
    {
        try
        {
            var sql = "SELECT COALESCE(SUM(FileSize), 0) FROM FileDocuments WHERE IsActive = 1";
            var dataTable = await LoadDataTableAsync(sql);
            
            if (!dataTable.IsSuccess || dataTable.Data.Rows.Count == 0)
                return Result<long>.Success(0);

            var totalSize = Convert.ToInt64(dataTable.Data.Rows[0][0]);
            return Result<long>.Success(totalSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total file size");
            return Result<long>.Failure($"Error calculating total file size: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetTotalDownloadCountAsync()
    {
        try
        {
            var sql = "SELECT COALESCE(SUM(DownloadCount), 0) FROM FileDocuments WHERE IsActive = 1";
            var dataTable = await LoadDataTableAsync(sql);
            
            if (!dataTable.IsSuccess || dataTable.Data.Rows.Count == 0)
                return Result<int>.Success(0);

            var totalDownloads = Convert.ToInt32(dataTable.Data.Rows[0][0]);
            return Result<int>.Success(totalDownloads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total download count");
            return Result<int>.Failure($"Error calculating total downloads: {ex.Message}");
        }
    }
}