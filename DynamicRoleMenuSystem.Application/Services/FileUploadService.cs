using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DynamicRoleMenuSystem.Application.Interfaces;

namespace DynamicRoleMenuSystem.Application.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;
    
    // Constants for file paths
    public const string PROFILE_IMAGES_FOLDER = "uploads/profiles";
    public const string DEFAULT_AVATAR_PATH = "/images/default-avatar.svg";
    
    // Allowed image extensions
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folderName = "uploads")
    {
        try
        {
            if (file == null || file.Length == 0)
                return null;

            if (!IsValidImage(file))
            {
                _logger.LogWarning("Invalid image file attempted to upload");
                return null;
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
            
            // Create full path
            var relativePath = Path.Combine(folderName, fileName);
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

            // Return relative path for storage in database
            return $"/{relativePath.Replace('\\', '/')}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return null;
        }
    }

    public async Task<bool> DeleteImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // Don't delete default avatar
            if (filePath.Equals(DEFAULT_AVATAR_PATH, StringComparison.OrdinalIgnoreCase))
                return false;

            // Remove leading slash if present
            if (filePath.StartsWith("/"))
                filePath = filePath.Substring(1);

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace('/', '\\'));

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {FilePath}", filePath);
            return false;
        }
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > MAX_FILE_SIZE)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
            return false;

        // Additional check for file content
        try
        {
            using (var stream = file.OpenReadStream())
            {
                // Read first few bytes to verify it's actually an image
                byte[] header = new byte[8];
                stream.Read(header, 0, header.Length);
                stream.Seek(0, SeekOrigin.Begin);

                // Check for common image file signatures
                if (IsJpeg(header) || IsPng(header) || IsGif(header) || IsWebP(header))
                    return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public string GetImageUrl(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return DEFAULT_AVATAR_PATH;

        // If it's already a full URL, return as is
        if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
            return filePath;

        // Ensure path starts with /
        if (!filePath.StartsWith("/"))
            filePath = "/" + filePath;

        return filePath;
    }

    private bool IsJpeg(byte[] bytes)
    {
        return bytes.Length >= 3 && 
               bytes[0] == 0xFF && 
               bytes[1] == 0xD8 && 
               bytes[2] == 0xFF;
    }

    private bool IsPng(byte[] bytes)
    {
        return bytes.Length >= 8 &&
               bytes[0] == 0x89 &&
               bytes[1] == 0x50 &&
               bytes[2] == 0x4E &&
               bytes[3] == 0x47 &&
               bytes[4] == 0x0D &&
               bytes[5] == 0x0A &&
               bytes[6] == 0x1A &&
               bytes[7] == 0x0A;
    }

    private bool IsGif(byte[] bytes)
    {
        return bytes.Length >= 6 &&
               bytes[0] == 0x47 && // G
               bytes[1] == 0x49 && // I
               bytes[2] == 0x46 && // F
               bytes[3] == 0x38 && // 8
               (bytes[4] == 0x37 || bytes[4] == 0x39) && // 7 or 9
               bytes[5] == 0x61; // a
    }

    private bool IsWebP(byte[] bytes)
    {
        return bytes.Length >= 4 &&
               bytes[0] == 0x52 && // R
               bytes[1] == 0x49 && // I
               bytes[2] == 0x46 && // F
               bytes[3] == 0x46; // F
    }
}