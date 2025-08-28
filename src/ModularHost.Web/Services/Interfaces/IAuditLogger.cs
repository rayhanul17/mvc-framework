using ModularHost.Web.Core.Models.Entities;
using System.Threading.Tasks;

namespace ModularHost.Web.Services.Interfaces
{
    public interface IAuditLogger
    {
        Task LogAsync(Log log);
        Task LogAsync(string level, string message, string exception = null, string properties = null);
        Task LogInfoAsync(string message, string properties = null);
        Task LogWarningAsync(string message, string properties = null);
        Task LogErrorAsync(string message, string exception = null, string properties = null);
    }
}