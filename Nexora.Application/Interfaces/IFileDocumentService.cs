using Microsoft.AspNetCore.Http;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using System.Data;

namespace Nexora.Application.Interfaces;

public interface IFileDocumentService : IBaseService<FileDocument>
{
    Task<Result<DataTable>> GetFileDocumentsForDataTableAsync(
        int draw, 
        int start, 
        int length, 
        string searchValue,
        int sortColumn,
        string sortDirection,
        string? category = null,
        string? fileExtension = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    Task<Result<FileDocument>> UploadFileAsync(IFormFile file, string category, string? description, string? tags, string uploadedBy);
    Task<Result<bool>> DeleteFileWithPhysicalRemovalAsync(int id);
    Task<Result<FileDocument>> IncrementDownloadCountAsync(int id);
    Task<Result<IEnumerable<string>>> GetCategoriesAsync();
    Task<Result<IEnumerable<string>>> GetFileExtensionsAsync();
    Task<Result<long>> GetTotalFileSizeAsync();
    Task<Result<int>> GetTotalDownloadCountAsync();
}