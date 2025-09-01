using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Core.Helpers
{
    public static class FileUploadHelper
    {
        // Allowed file extensions for different types
        private static readonly Dictionary<string, List<string>> AllowedExtensions = new()
        {
            ["image"] = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" },
            ["document"] = new List<string> { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv" },
            ["video"] = new List<string> { ".mp4", ".avi", ".mov", ".wmv", ".webm" },
            ["audio"] = new List<string> { ".mp3", ".wav", ".ogg", ".m4a" },
            ["archive"] = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz" }
        };

        // Maximum file sizes in bytes (default 10MB)
        private static readonly Dictionary<string, long> MaxFileSizes = new()
        {
            ["image"] = 5 * 1024 * 1024,      // 5MB
            ["document"] = 10 * 1024 * 1024,   // 10MB
            ["video"] = 100 * 1024 * 1024,     // 100MB
            ["audio"] = 20 * 1024 * 1024,      // 20MB
            ["archive"] = 50 * 1024 * 1024     // 50MB
        };

        /// <summary>
        /// Upload a single file
        /// </summary>
        public static async Task<FileUploadResult> UploadFileAsync(
            IFormFile file,
            string uploadPath,
            string fileType = "document",
            bool generateUniqueName = true)
        {
            var result = new FileUploadResult();

            try
            {
                if (file == null || file.Length == 0)
                {
                    result.Success = false;
                    result.ErrorMessage = "No file uploaded";
                    return result;
                }

                // Validate file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!IsValidExtension(extension, fileType))
                {
                    result.Success = false;
                    result.ErrorMessage = $"File type {extension} is not allowed for {fileType}";
                    return result;
                }

                // Validate file size
                if (!IsValidSize(file.Length, fileType))
                {
                    result.Success = false;
                    result.ErrorMessage = $"File size exceeds the maximum allowed size for {fileType}";
                    return result;
                }

                // Generate file name
                var fileName = generateUniqueName
                    ? $"{Guid.NewGuid()}{extension}"
                    : SanitizeFileName(file.FileName);

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Full file path
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                result.Success = true;
                result.FileName = fileName;
                result.FilePath = filePath;
                result.FileSize = file.Length;
                result.ContentType = file.ContentType;
                result.OriginalFileName = file.FileName;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error uploading file: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        public static async Task<List<FileUploadResult>> UploadFilesAsync(
            IFormFileCollection files,
            string uploadPath,
            string fileType = "document",
            bool generateUniqueName = true)
        {
            var results = new List<FileUploadResult>();

            foreach (var file in files)
            {
                var result = await UploadFileAsync(file, uploadPath, fileType, generateUniqueName);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get file URL for web access
        /// </summary>
        public static string GetFileUrl(string fileName, string baseUrl, string folder)
        {
            return $"{baseUrl.TrimEnd('/')}/{folder.Trim('/')}/{fileName}";
        }

        /// <summary>
        /// Create thumbnail for image
        /// </summary>
        public static async Task<string> CreateThumbnailAsync(
            string imagePath,
            string thumbnailPath,
            int width = 300,
            int height = 300)
        {
            // Note: This would require an image processing library like ImageSharp
            // For now, returning the original path
            await Task.CompletedTask;
            return imagePath;
        }

        /// <summary>
        /// Validate file extension
        /// </summary>
        private static bool IsValidExtension(string extension, string fileType)
        {
            if (AllowedExtensions.ContainsKey(fileType))
            {
                return AllowedExtensions[fileType].Contains(extension);
            }

            // If type not found, check all allowed extensions
            return AllowedExtensions.Values.Any(list => list.Contains(extension));
        }

        /// <summary>
        /// Validate file size
        /// </summary>
        private static bool IsValidSize(long fileSize, string fileType)
        {
            if (MaxFileSizes.ContainsKey(fileType))
            {
                return fileSize <= MaxFileSizes[fileType];
            }

            // Default max size 10MB
            return fileSize <= 10 * 1024 * 1024;
        }

        /// <summary>
        /// Sanitize file name to remove invalid characters
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Ensure the file has an extension
            if (!Path.HasExtension(sanitized))
            {
                sanitized += ".unknown";
            }

            return sanitized;
        }

        /// <summary>
        /// Get file icon based on extension
        /// </summary>
        public static string GetFileIcon(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "fas fa-file-pdf text-red-500",
                ".doc" or ".docx" => "fas fa-file-word text-blue-500",
                ".xls" or ".xlsx" => "fas fa-file-excel text-green-500",
                ".ppt" or ".pptx" => "fas fa-file-powerpoint text-orange-500",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "fas fa-file-image text-purple-500",
                ".mp4" or ".avi" or ".mov" => "fas fa-file-video text-indigo-500",
                ".mp3" or ".wav" => "fas fa-file-audio text-pink-500",
                ".zip" or ".rar" or ".7z" => "fas fa-file-archive text-yellow-500",
                ".txt" => "fas fa-file-alt text-gray-500",
                ".csv" => "fas fa-file-csv text-teal-500",
                _ => "fas fa-file text-gray-400"
            };
        }

        /// <summary>
        /// Format file size for display
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// File upload result
    /// </summary>
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ThumbnailPath { get; set; }
    }
}