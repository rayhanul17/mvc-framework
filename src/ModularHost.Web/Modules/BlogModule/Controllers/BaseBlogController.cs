using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MRCMS.Core.Infrastructure.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRCMS.Modules.BlogModule.Controllers
{
    /// <summary>
    /// Optimized base controller for blog module with high-performance error handling
    /// </summary>
    public abstract class BaseBlogController : Controller
    {
        protected readonly ILogger _logger;
        
        // Pre-compiled action messages for common operations
        private static readonly Dictionary<string, string> ActionMessages = new()
        {
            ["create_success"] = "Item created successfully",
            ["update_success"] = "Item updated successfully",
            ["delete_success"] = "Item deleted successfully",
            ["not_found"] = "Item not found",
            ["invalid_data"] = "Invalid data provided",
            ["unauthorized"] = "You are not authorized to perform this action",
            ["operation_failed"] = "Operation failed. Please try again."
        };

        protected BaseBlogController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute action with automatic error handling
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync<T>(
            Func<Task<T>> action,
            string successMessage = null)
        {
            try
            {
                var result = await action();
                return ResultHandler.Success(result, successMessage);
            }
            catch (KeyNotFoundException ex)
            {
                LogError("Resource not found", ex);
                return ResultHandler.NotFound(ActionMessages["not_found"]);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError("Unauthorized access", ex);
                return ResultHandler.Error(ActionMessages["unauthorized"], 401);
            }
            catch (ArgumentException ex)
            {
                LogError("Invalid argument", ex);
                return ResultHandler.Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                LogError("Unexpected error", ex);
                return ResultHandler.Error(ActionMessages["operation_failed"], 500);
            }
        }

        /// <summary>
        /// Execute action without return value
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync(
            Func<Task> action,
            string successMessage = null)
        {
            try
            {
                await action();
                return ResultHandler.Success(successMessage);
            }
            catch (KeyNotFoundException ex)
            {
                LogError("Resource not found", ex);
                return ResultHandler.NotFound(ActionMessages["not_found"]);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError("Unauthorized access", ex);
                return ResultHandler.Error(ActionMessages["unauthorized"], 401);
            }
            catch (ArgumentException ex)
            {
                LogError("Invalid argument", ex);
                return ResultHandler.Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                LogError("Unexpected error", ex);
                return ResultHandler.Error(ActionMessages["operation_failed"], 500);
            }
        }

        /// <summary>
        /// Validate model state and return appropriate error response
        /// </summary>
        protected IActionResult ValidateModel()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return ResultHandler.ValidationError(errors, ActionMessages["invalid_data"]);
            }

            return null;
        }

        /// <summary>
        /// Handle paginated results efficiently
        /// </summary>
        protected IActionResult Paginated<T>(
            IEnumerable<T> items,
            int page,
            int pageSize,
            int totalCount)
        {
            return ResultHandler.Paginated(items, page, pageSize, totalCount);
        }

        /// <summary>
        /// Get message from pre-compiled dictionary
        /// </summary>
        protected string GetMessage(string key)
        {
            return ActionMessages.TryGetValue(key, out var message) 
                ? message 
                : "Operation completed";
        }

        /// <summary>
        /// Set success message in TempData
        /// </summary>
        protected void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Set error message in TempData
        /// </summary>
        protected void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        /// <summary>
        /// Log error with context
        /// </summary>
        private void LogError(string context, Exception ex)
        {
            _logger.LogError(ex, "{Context} - Controller: {Controller}, Action: {Action}, User: {User}",
                context,
                ControllerContext.ActionDescriptor.ControllerName,
                ControllerContext.ActionDescriptor.ActionName,
                User?.Identity?.Name ?? "Anonymous");
        }

        /// <summary>
        /// Redirect with message
        /// </summary>
        protected IActionResult RedirectWithMessage(string action, string controller, string message, bool isError = false)
        {
            if (isError)
                SetErrorMessage(message);
            else
                SetSuccessMessage(message);

            return RedirectToAction(action, controller);
        }

        /// <summary>
        /// Check if request is AJAX
        /// </summary>
        protected bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// Return JSON or View based on request type
        /// </summary>
        protected IActionResult SmartResult<T>(T data, string viewName = null)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, data });
            }

            return View(viewName, data);
        }

        /// <summary>
        /// Return error as JSON or View based on request type
        /// </summary>
        protected IActionResult SmartError(string message, int statusCode = 400)
        {
            if (IsAjaxRequest())
            {
                Response.StatusCode = statusCode;
                return Json(new { success = false, message });
            }

            SetErrorMessage(message);
            return View("Error");
        }
    }
}