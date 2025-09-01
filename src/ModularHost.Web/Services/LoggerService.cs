using Microsoft.Extensions.Configuration;
using MRCMS.Services.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MRCMS.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly string _adminEmail;
        private readonly string _applicationName;
        private readonly bool _enableEmailNotifications;

        public LoggerService(
            Serilog.ILogger logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
            
            _adminEmail = _configuration["Logging:AdminEmail"] ?? "admin@example.com";
            _applicationName = _configuration["Application:Name"] ?? "ModularHost CMS";
            _enableEmailNotifications = _configuration.GetValue<bool>("Logging:EnableEmailNotifications", false);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        public void LogError(string message, Exception? exception = null, params object[] args)
        {
            if (exception != null)
            {
                _logger.Error(exception, message, args);
            }
            else
            {
                _logger.Error(message, args);
            }
        }

        public void LogCritical(string message, Exception? exception = null, params object[] args)
        {
            if (exception != null)
            {
                _logger.Fatal(exception, message, args);
            }
            else
            {
                _logger.Fatal(message, args);
            }
        }

        public async Task LogCriticalWithEmailAsync(string message, Exception? exception = null, params object[] args)
        {
            // Log to file first
            LogCritical(message, exception, args);

            // Send email notification if enabled
            if (_enableEmailNotifications)
            {
                try
                {
                    var emailBody = BuildCriticalErrorEmailBody(message, exception, args);
                    var subject = $"[CRITICAL] {_applicationName} - {message}";
                    
                    await _emailService.SendEmailAsync(
                        _adminEmail,
                        subject,
                        emailBody);
                    
                    _logger.Information("Critical error email notification sent to {AdminEmail}", _adminEmail);
                }
                catch (Exception emailEx)
                {
                    // Don't throw if email fails, just log it
                    _logger.Error(emailEx, "Failed to send critical error email notification");
                }
            }
        }

        public async Task LogStartupAsync(string message, params object[] args)
        {
            // Log to file
            _logger.Information($"[STARTUP] {message}", args);

            // Send startup notification email if enabled
            if (_enableEmailNotifications)
            {
                try
                {
                    var emailBody = BuildStartupEmailBody(message, args);
                    var subject = $"[STARTUP] {_applicationName} - Application Started";
                    
                    await _emailService.SendEmailAsync(
                        _adminEmail,
                        subject,
                        emailBody);
                    
                    _logger.Information("Startup email notification sent to {AdminEmail}", _adminEmail);
                }
                catch (Exception emailEx)
                {
                    // Don't fail startup if email fails
                    _logger.Warning(emailEx, "Failed to send startup email notification");
                }
            }
        }

        public void LogWithContext(string message, object context, Interfaces.LogLevel level = Interfaces.LogLevel.Information)
        {
            var contextualLogger = _logger.ForContext("Context", context, destructureObjects: true);
            
            switch (level)
            {
                case Interfaces.LogLevel.Debug:
                    contextualLogger.Debug(message);
                    break;
                case Interfaces.LogLevel.Information:
                    contextualLogger.Information(message);
                    break;
                case Interfaces.LogLevel.Warning:
                    contextualLogger.Warning(message);
                    break;
                case Interfaces.LogLevel.Error:
                    contextualLogger.Error(message);
                    break;
                case Interfaces.LogLevel.Critical:
                    contextualLogger.Fatal(message);
                    break;
            }
        }

        private string BuildCriticalErrorEmailBody(string message, Exception? exception, object[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body>");
            sb.AppendLine($"<h2 style='color: red;'>Critical Error in {_applicationName}</h2>");
            sb.AppendLine($"<p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
            sb.AppendLine($"<p><strong>Server:</strong> {Environment.MachineName}</p>");
            sb.AppendLine($"<p><strong>Message:</strong> {string.Format(message, args)}</p>");
            
            if (exception != null)
            {
                sb.AppendLine("<h3>Exception Details:</h3>");
                sb.AppendLine("<pre style='background-color: #f5f5f5; padding: 10px;'>");
                sb.AppendLine($"Type: {exception.GetType().FullName}");
                sb.AppendLine($"Message: {exception.Message}");
                sb.AppendLine($"Stack Trace:\n{exception.StackTrace}");
                
                if (exception.InnerException != null)
                {
                    sb.AppendLine($"\nInner Exception:");
                    sb.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
                    sb.AppendLine($"Message: {exception.InnerException.Message}");
                    sb.AppendLine($"Stack Trace:\n{exception.InnerException.StackTrace}");
                }
                sb.AppendLine("</pre>");
            }
            
            sb.AppendLine("<p style='margin-top: 20px; color: #666;'>This is an automated message. Please check the application logs for more details.</p>");
            sb.AppendLine("</body></html>");
            
            return sb.ToString();
        }

        private string BuildStartupEmailBody(string message, object[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><body>");
            sb.AppendLine($"<h2 style='color: green;'>{_applicationName} Started Successfully</h2>");
            sb.AppendLine($"<p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
            sb.AppendLine($"<p><strong>Server:</strong> {Environment.MachineName}</p>");
            sb.AppendLine($"<p><strong>Message:</strong> {string.Format(message, args)}</p>");
            
            sb.AppendLine("<h3>Environment Information:</h3>");
            sb.AppendLine("<ul>");
            sb.AppendLine($"<li>Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}</li>");
            sb.AppendLine($"<li>OS: {Environment.OSVersion}</li>");
            sb.AppendLine($"<li>.NET Version: {Environment.Version}</li>");
            sb.AppendLine($"<li>Processor Count: {Environment.ProcessorCount}</li>");
            sb.AppendLine("</ul>");
            
            sb.AppendLine("<p style='margin-top: 20px; color: #666;'>This is an automated startup notification.</p>");
            sb.AppendLine("</body></html>");
            
            return sb.ToString();
        }
    }
}