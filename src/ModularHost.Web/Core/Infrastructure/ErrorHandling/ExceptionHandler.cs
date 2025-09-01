using System;
using System.Collections.Generic;
using System.Net;
using MRCMS.Core.Exceptions;

namespace MRCMS.Core.Infrastructure.ErrorHandling
{
    /// <summary>
    /// High-performance exception handler using dictionary lookups instead of switch statements
    /// </summary>
    public static class ExceptionHandler
    {
        // Pre-compiled exception type mappings for O(1) lookup performance
        private static readonly Dictionary<Type, Func<Exception, ErrorDetails>> ExceptionMappings = new()
        {
            // Custom application exceptions
            [typeof(NotFoundException)] = ex => new ErrorDetails(404, ex.Message, ((NotFoundException)ex).Details),
            [typeof(UnauthorizedException)] = ex => new ErrorDetails(401, ex.Message, ((UnauthorizedException)ex).Details),
            [typeof(ForbiddenException)] = ex => new ErrorDetails(403, ex.Message, ((ForbiddenException)ex).Details),
            [typeof(ValidationException)] = ex => new ErrorDetails(400, ex.Message, ((ValidationException)ex).Details),
            [typeof(ConflictException)] = ex => new ErrorDetails(409, ex.Message, ((ConflictException)ex).Details),
            [typeof(BadRequestException)] = ex => new ErrorDetails(400, ex.Message, ((BadRequestException)ex).Details),
            [typeof(MRCMS.Core.Exceptions.ApplicationException)] = ex => 
            {
                var appEx = (MRCMS.Core.Exceptions.ApplicationException)ex;
                return new ErrorDetails(appEx.StatusCode, appEx.Message, appEx.Details);
            },
            
            // System exceptions
            [typeof(UnauthorizedAccessException)] = ex => new ErrorDetails(401, "Authentication required", "You must be logged in to access this resource."),
            [typeof(ArgumentNullException)] = ex => new ErrorDetails(400, "Invalid request", ex.Message),
            [typeof(ArgumentException)] = ex => new ErrorDetails(400, "Invalid request", ex.Message),
            [typeof(InvalidOperationException)] = ex => new ErrorDetails(400, "Invalid operation", ex.Message),
            [typeof(NotImplementedException)] = ex => new ErrorDetails(501, "Not implemented", "This feature is not yet implemented."),
            [typeof(TimeoutException)] = ex => new ErrorDetails(408, "Request timeout", "The request took too long to complete."),
            [typeof(OperationCanceledException)] = ex => new ErrorDetails(499, "Request cancelled", "The operation was cancelled."),
            [typeof(NotSupportedException)] = ex => new ErrorDetails(405, "Method not allowed", ex.Message),
            [typeof(KeyNotFoundException)] = ex => new ErrorDetails(404, "Resource not found", ex.Message),
            [typeof(FileNotFoundException)] = ex => new ErrorDetails(404, "File not found", ex.Message),
            [typeof(DirectoryNotFoundException)] = ex => new ErrorDetails(404, "Directory not found", ex.Message),
            [typeof(UnauthorizedAccessException)] = ex => new ErrorDetails(403, "Access denied", "You don't have permission to access this resource."),
            [typeof(AccessViolationException)] = ex => new ErrorDetails(403, "Access violation", "Access to this resource is forbidden."),
            [typeof(OutOfMemoryException)] = ex => new ErrorDetails(507, "Insufficient storage", "The server has insufficient storage to complete the request."),
            [typeof(StackOverflowException)] = ex => new ErrorDetails(500, "Stack overflow", "A stack overflow occurred while processing your request."),
            [typeof(DivideByZeroException)] = ex => new ErrorDetails(500, "Calculation error", "An error occurred during calculation."),
            [typeof(IndexOutOfRangeException)] = ex => new ErrorDetails(500, "Index error", "An internal error occurred."),
            [typeof(NullReferenceException)] = ex => new ErrorDetails(500, "Reference error", "An internal error occurred."),
            [typeof(FormatException)] = ex => new ErrorDetails(400, "Invalid format", "The provided data is in an invalid format."),
            [typeof(InvalidCastException)] = ex => new ErrorDetails(400, "Type error", "Invalid data type provided."),
        };

        // Status code to error page mappings
        private static readonly Dictionary<int, string> ErrorPageMappings = new()
        {
            [400] = "/Error/BadRequest",
            [401] = "/Error/Unauthorized",
            [403] = "/Error/Forbidden",
            [404] = "/Error/NotFound",
            [405] = "/Error/MethodNotAllowed",
            [408] = "/Error/Timeout",
            [409] = "/Error/Conflict",
            [500] = "/Error/Index",
            [501] = "/Error/NotImplemented",
            [502] = "/Error/BadGateway",
            [503] = "/Error/ServiceUnavailable",
            [504] = "/Error/GatewayTimeout"
        };

        // Default error details for unknown exceptions
        private static readonly ErrorDetails DefaultErrorDetails = new(500, "An error occurred", "An unexpected error occurred. Please try again later.");
        private static readonly ErrorDetails DefaultDevErrorDetails = new(500, "Internal Server Error", null);

        /// <summary>
        /// Get error details from exception using high-performance dictionary lookup
        /// </summary>
        public static ErrorDetails GetErrorDetails(Exception exception, bool isDevelopment = false)
        {
            if (exception == null)
                return DefaultErrorDetails;

            // Try exact type match first (O(1) lookup)
            var exceptionType = exception.GetType();
            if (ExceptionMappings.TryGetValue(exceptionType, out var handler))
            {
                return handler(exception);
            }

            // Try base type matches for inherited exceptions
            foreach (var mapping in ExceptionMappings)
            {
                if (mapping.Key.IsAssignableFrom(exceptionType))
                {
                    return mapping.Value(exception);
                }
            }

            // Return default error details
            return isDevelopment 
                ? new ErrorDetails(500, exception.Message, exception.ToString())
                : DefaultErrorDetails;
        }

        /// <summary>
        /// Get error page path for status code
        /// </summary>
        public static string GetErrorPagePath(int statusCode)
        {
            return ErrorPageMappings.TryGetValue(statusCode, out var path) 
                ? path 
                : "/Error/Index";
        }

        /// <summary>
        /// Register custom exception mapping
        /// </summary>
        public static void RegisterExceptionMapping<TException>(Func<TException, ErrorDetails> handler) 
            where TException : Exception
        {
            ExceptionMappings[typeof(TException)] = ex => handler((TException)ex);
        }

        /// <summary>
        /// Register custom error page mapping
        /// </summary>
        public static void RegisterErrorPageMapping(int statusCode, string path)
        {
            ErrorPageMappings[statusCode] = path;
        }
    }

    /// <summary>
    /// Error details structure for fast access
    /// </summary>
    public readonly struct ErrorDetails
    {
        public readonly int StatusCode;
        public readonly string Message;
        public readonly string Details;

        public ErrorDetails(int statusCode, string message, string details = null)
        {
            StatusCode = statusCode;
            Message = message ?? "An error occurred";
            Details = details;
        }
    }
}