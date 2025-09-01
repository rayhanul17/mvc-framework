using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRCMS.Core.Infrastructure.Results
{
    /// <summary>
    /// High-performance result handler for consistent API responses
    /// </summary>
    public static class ResultHandler
    {
        // Pre-compiled status messages for common scenarios
        private static readonly Dictionary<int, string> StatusMessages = new()
        {
            [200] = "Success",
            [201] = "Created",
            [204] = "No Content",
            [400] = "Bad Request",
            [401] = "Unauthorized",
            [403] = "Forbidden",
            [404] = "Not Found",
            [409] = "Conflict",
            [500] = "Internal Server Error"
        };

        /// <summary>
        /// Create success result with data
        /// </summary>
        public static IActionResult Success<T>(T data, string message = null)
        {
            return new OkObjectResult(new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? StatusMessages[200],
                StatusCode = 200
            });
        }

        /// <summary>
        /// Create success result without data
        /// </summary>
        public static IActionResult Success(string message = null)
        {
            return new OkObjectResult(new ApiResponse
            {
                Success = true,
                Message = message ?? StatusMessages[200],
                StatusCode = 200
            });
        }

        /// <summary>
        /// Create created result
        /// </summary>
        public static IActionResult Created<T>(T data, string location = null)
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = StatusMessages[201],
                StatusCode = 201
            };

            if (!string.IsNullOrEmpty(location))
            {
                return new CreatedResult(location, response);
            }

            return new ObjectResult(response)
            {
                StatusCode = 201
            };
        }

        /// <summary>
        /// Create error result
        /// </summary>
        public static IActionResult Error(string message, int statusCode = 400, Dictionary<string, string[]> errors = null)
        {
            return new ObjectResult(new ApiResponse
            {
                Success = false,
                Message = message ?? StatusMessages.GetValueOrDefault(statusCode, "Error"),
                StatusCode = statusCode,
                Errors = errors
            })
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Create not found result
        /// </summary>
        public static IActionResult NotFound(string message = null)
        {
            return new NotFoundObjectResult(new ApiResponse
            {
                Success = false,
                Message = message ?? StatusMessages[404],
                StatusCode = 404
            });
        }

        /// <summary>
        /// Create validation error result
        /// </summary>
        public static IActionResult ValidationError(Dictionary<string, string[]> errors, string message = null)
        {
            return new BadRequestObjectResult(new ApiResponse
            {
                Success = false,
                Message = message ?? "Validation failed",
                StatusCode = 400,
                Errors = errors
            });
        }

        /// <summary>
        /// Create paginated result
        /// </summary>
        public static IActionResult Paginated<T>(IEnumerable<T> items, int page, int pageSize, int totalCount)
        {
            return new OkObjectResult(new PaginatedResponse<T>
            {
                Success = true,
                Data = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Message = StatusMessages[200],
                StatusCode = 200
            });
        }
    }

    /// <summary>
    /// Standard API response structure
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// API response with data
    /// </summary>
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }

    /// <summary>
    /// Paginated API response
    /// </summary>
    public class PaginatedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    /// <summary>
    /// Operation result for service layer
    /// </summary>
    public class OperationResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string Error { get; private set; }
        public Dictionary<string, string[]> ValidationErrors { get; private set; }

        private OperationResult(bool isSuccess, T data = default, string error = null, Dictionary<string, string[]> validationErrors = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
            ValidationErrors = validationErrors;
        }

        public static OperationResult<T> Success(T data) => new(true, data);
        public static OperationResult<T> Failure(string error) => new(false, error: error);
        public static OperationResult<T> ValidationFailure(Dictionary<string, string[]> errors) => new(false, validationErrors: errors);
        
        public IActionResult ToActionResult()
        {
            if (IsSuccess)
                return ResultHandler.Success(Data);
            
            if (ValidationErrors != null)
                return ResultHandler.ValidationError(ValidationErrors, Error);
                
            return ResultHandler.Error(Error);
        }
    }

    /// <summary>
    /// Async operation result
    /// </summary>
    public class AsyncOperationResult<T>
    {
        private readonly Task<OperationResult<T>> _task;

        public AsyncOperationResult(Task<OperationResult<T>> task)
        {
            _task = task;
        }

        public static implicit operator AsyncOperationResult<T>(Task<OperationResult<T>> task)
        {
            return new AsyncOperationResult<T>(task);
        }

        public async Task<IActionResult> ToActionResultAsync()
        {
            var result = await _task;
            return result.ToActionResult();
        }
    }
}