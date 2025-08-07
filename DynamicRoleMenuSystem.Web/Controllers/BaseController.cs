using Microsoft.AspNetCore.Mvc;
using DynamicRoleMenuSystem.Core.Common;

namespace DynamicRoleMenuSystem.Web.Controllers;

public abstract class BaseController : Controller
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
        {
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
            
            return BadRequest(ModelState);
        }

        return Ok(result.Data);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (!result.IsSuccess)
        {
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
            
            return BadRequest(ModelState);
        }

        return Ok();
    }

    protected void AddErrorsToModelState(Result result)
    {
        if (!result.IsSuccess)
        {
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
        }
    }

    protected void AddErrorsToModelState<T>(Result<T> result)
    {
        if (!result.IsSuccess)
        {
            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }
        }
    }

    protected void SetSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    protected void SetErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    protected string GetCurrentUserId()
    {
        return User.Identity?.IsAuthenticated == true 
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty
            : string.Empty;
    }

    protected string GetCurrentUserName()
    {
        return User.Identity?.Name ?? string.Empty;
    }
}