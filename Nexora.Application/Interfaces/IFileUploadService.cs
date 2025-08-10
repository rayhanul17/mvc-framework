using Microsoft.AspNetCore.Http;

namespace Nexora.Application.Interfaces;

public interface IFileUploadService
{
    Task<string?> UploadImageAsync(IFormFile file, string folderName = "uploads");
    Task<bool> DeleteImageAsync(string filePath);
    bool IsValidImage(IFormFile file);
    string GetImageUrl(string? filePath);
}