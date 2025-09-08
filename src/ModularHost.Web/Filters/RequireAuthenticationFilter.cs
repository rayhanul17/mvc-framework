using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace MRCMS.Filters
{
    public class RequireAuthenticationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                // Check if it's an AJAX request
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    context.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    context.Result = new JsonResult(new { error = "Unauthorized", message = "Authentication required" })
                    {
                        StatusCode = 401
                    };
                }
                else
                {
                    var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                    context.Result = new RedirectToActionResult("Login", "Account", 
                        new { returnUrl = returnUrl });
                }
                return;
            }

            await next();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAuthenticationAttribute : TypeFilterAttribute
    {
        public RequireAuthenticationAttribute() : base(typeof(RequireAuthenticationFilter))
        {
        }
    }
}