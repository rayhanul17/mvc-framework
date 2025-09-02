namespace MRCMS.Models.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; } = 500;
    public string Message { get; set; } = "An error occurred";
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public string? RequestedPath { get; set; }
    public bool ShowDetails { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowStackTrace => ShowDetails && !string.IsNullOrEmpty(StackTrace);
    
    public string StatusCodeDisplay => StatusCode switch
    {
        400 => "400 - Bad Request",
        401 => "401 - Unauthorized",
        403 => "403 - Forbidden",
        404 => "404 - Not Found",
        500 => "500 - Internal Server Error",
        503 => "503 - Service Unavailable",
        _ => $"{StatusCode} - Error"
    };

    public string IconClass => StatusCode switch
    {
        404 => "fa-search",
        401 or 403 => "fa-lock",
        500 or 503 => "fa-server",
        _ => "fa-exclamation-triangle"
    };
}
