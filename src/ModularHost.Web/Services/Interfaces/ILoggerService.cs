using System;
using System.Threading.Tasks;

namespace MRCMS.Services.Interfaces
{
    public interface ILoggerService
    {
        void LogDebug(string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, Exception? exception = null, params object[] args);
        void LogCritical(string message, Exception? exception = null, params object[] args);
        
        // Email notification methods
        Task LogCriticalWithEmailAsync(string message, Exception? exception = null, params object[] args);
        Task LogStartupAsync(string message, params object[] args);
        
        // Structured logging
        void LogWithContext(string message, object context, LogLevel level = LogLevel.Information);
    }
    
    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }
}