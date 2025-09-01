using System;

namespace MRCMS.Core.Exceptions
{
    /// <summary>
    /// Base exception for all custom application exceptions
    /// </summary>
    public class ApplicationException : Exception
    {
        public int StatusCode { get; set; }
        public string? Details { get; set; }

        public ApplicationException(string message, int statusCode = 500, string? details = null) 
            : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }

        public ApplicationException(string message, Exception innerException, int statusCode = 500, string? details = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }

    /// <summary>
    /// Exception thrown when a requested resource is not found
    /// </summary>
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message, string? details = null)
            : base(message, 404, details)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} with id '{key}' was not found.", 404)
        {
        }
    }

    /// <summary>
    /// Exception thrown when user is not authorized to access a resource
    /// </summary>
    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException(string message = "You are not authorized to access this resource.", string? details = null)
            : base(message, 401, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown when user doesn't have permission for an action
    /// </summary>
    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException(string message = "You don't have permission to perform this action.", string? details = null)
            : base(message, 403, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown for validation errors
    /// </summary>
    public class ValidationException : ApplicationException
    {
        public ValidationException(string message, string? details = null)
            : base(message, 400, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown for business logic violations
    /// </summary>
    public class BusinessException : ApplicationException
    {
        public BusinessException(string message, string? details = null)
            : base(message, 400, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown when there's a conflict with existing data
    /// </summary>
    public class ConflictException : ApplicationException
    {
        public ConflictException(string message, string? details = null)
            : base(message, 409, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown for configuration errors
    /// </summary>
    public class ConfigurationException : ApplicationException
    {
        public ConfigurationException(string message, string? details = null)
            : base(message, 500, details)
        {
        }
    }

    /// <summary>
    /// Exception thrown for external service errors
    /// </summary>
    public class ExternalServiceException : ApplicationException
    {
        public string ServiceName { get; set; }

        public ExternalServiceException(string serviceName, string message, string? details = null)
            : base($"External service '{serviceName}' error: {message}", 503, details)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// Exception thrown for bad request errors
    /// </summary>
    public class BadRequestException : ApplicationException
    {
        public BadRequestException(string message, string? details = null)
            : base(message, 400, details)
        {
        }
    }
}