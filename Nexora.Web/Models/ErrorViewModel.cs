namespace Nexora.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; } = 500;
    public string? OriginalPath { get; set; }
    public string SiteName { get; set; } = "Nexora Framework";
    public string? ErrorMessage { get; set; }
    public string? ExceptionType { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowTechnicalDetails => !string.IsNullOrEmpty(ErrorMessage);
    
    public string GetStatusCodeDescription()
    {
        return StatusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Access Denied",
            404 => "Page Not Found",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            _ => "Error"
        };
    }
    
    public string GetUserFriendlyMessage()
    {
        return StatusCode switch
        {
            404 => "The page you're looking for doesn't exist or may have been moved.",
            403 => "You don't have permission to access this resource.",
            500 => "Something went wrong on our end. We're working to fix it.",
            502 => "We're having trouble connecting to our servers.",
            503 => "The service is temporarily unavailable.",
            _ => "An unexpected error occurred."
        };
    }
}
