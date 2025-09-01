using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MRCMS.Core.Extensions
{
    public static class RazorPageExtensions
    {
        /// <summary>
        /// Check if the current user is a SuperAdmin
        /// Usage in Razor: @Html.IsSuperAdmin()
        /// </summary>
        public static bool IsSuperAdmin(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.HasClaim("IsSuperAdmin", "true") ?? false;
        }
        
        /// <summary>
        /// Check if the current user is authenticated
        /// Usage in Razor: @Html.IsAuthenticated()
        /// </summary>
        public static bool IsAuthenticated(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.Identity?.IsAuthenticated ?? false;
        }
        
        /// <summary>
        /// Check if the current user is in a specific role
        /// Usage in Razor: @Html.IsInRole("Admin")
        /// </summary>
        public static bool IsInRole(this IHtmlHelper htmlHelper, string role)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.IsInRole(role) ?? false;
        }
        
        /// <summary>
        /// Check if the current user is an Admin (Admin, Administrator, or SuperAdmin)
        /// Usage in Razor: @Html.IsAdmin()
        /// </summary>
        public static bool IsAdmin(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            if (user == null) return false;
            
            return user.IsInRole("Admin") || 
                   user.IsInRole("Administrator") || 
                   user.HasClaim("IsSuperAdmin", "true");
        }
        
        /// <summary>
        /// Get the current user's ID
        /// Usage in Razor: @Html.GetUserId()
        /// </summary>
        public static string GetUserId(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }
        
        /// <summary>
        /// Get the current user's username
        /// Usage in Razor: @Html.GetUserName()
        /// </summary>
        public static string GetUserName(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.FindFirst("UserName")?.Value ?? user?.Identity?.Name ?? "";
        }
        
        /// <summary>
        /// Get the current user's full name
        /// Usage in Razor: @Html.GetFullName()
        /// </summary>
        public static string GetFullName(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.FindFirst("FullName")?.Value ?? "";
        }
        
        /// <summary>
        /// Get the current user's avatar URL
        /// Usage in Razor: @Html.GetAvatarUrl()
        /// </summary>
        public static string GetAvatarUrl(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user?.FindFirst("AvatarUrl")?.Value ?? "/images/default-avatar.png";
        }
        
        /// <summary>
        /// Render content only if user is SuperAdmin
        /// Usage in Razor: @Html.IfSuperAdmin("<div>SuperAdmin only content</div>")
        /// </summary>
        public static IHtmlContent IfSuperAdmin(this IHtmlHelper htmlHelper, string content)
        {
            if (htmlHelper.IsSuperAdmin())
            {
                return new HtmlString(content);
            }
            return HtmlString.Empty;
        }
        
        /// <summary>
        /// Render content only if user is SuperAdmin (with Func)
        /// Usage in Razor: @Html.IfSuperAdmin(() => Html.ActionLink("Admin", "Index", "Admin"))
        /// </summary>
        public static IHtmlContent IfSuperAdmin(this IHtmlHelper htmlHelper, Func<IHtmlContent> content)
        {
            if (htmlHelper.IsSuperAdmin())
            {
                return content();
            }
            return HtmlString.Empty;
        }
        
        /// <summary>
        /// Render content only if user is authenticated
        /// Usage in Razor: @Html.IfAuthenticated("<div>Logged in content</div>")
        /// </summary>
        public static IHtmlContent IfAuthenticated(this IHtmlHelper htmlHelper, string content)
        {
            if (htmlHelper.IsAuthenticated())
            {
                return new HtmlString(content);
            }
            return HtmlString.Empty;
        }
        
        /// <summary>
        /// Render content only if user is in role
        /// Usage in Razor: @Html.IfInRole("Admin", "<div>Admin content</div>")
        /// </summary>
        public static IHtmlContent IfInRole(this IHtmlHelper htmlHelper, string role, string content)
        {
            if (htmlHelper.IsInRole(role))
            {
                return new HtmlString(content);
            }
            return HtmlString.Empty;
        }
        
        /// <summary>
        /// Get the base URL of the application
        /// Usage in Razor: @Html.GetBaseUrl()
        /// </summary>
        public static string GetBaseUrl(this IHtmlHelper htmlHelper)
        {
            var request = htmlHelper.ViewContext.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}";
        }
        
        /// <summary>
        /// Get the current page URL
        /// Usage in Razor: @Html.GetCurrentUrl()
        /// </summary>
        public static string GetCurrentUrl(this IHtmlHelper htmlHelper)
        {
            var request = htmlHelper.ViewContext.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        }
        
        /// <summary>
        /// Check if the current request is an AJAX request
        /// Usage in Razor: @Html.IsAjaxRequest()
        /// </summary>
        public static bool IsAjaxRequest(this IHtmlHelper htmlHelper)
        {
            var request = htmlHelper.ViewContext.HttpContext.Request;
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
        
        /// <summary>
        /// Get a value from TempData safely
        /// Usage in Razor: @Html.GetTempData("Success")
        /// </summary>
        public static string GetTempData(this IHtmlHelper htmlHelper, string key)
        {
            if (htmlHelper.ViewContext.TempData.ContainsKey(key))
            {
                return htmlHelper.ViewContext.TempData[key]?.ToString() ?? "";
            }
            return "";
        }
        
        /// <summary>
        /// Get a value from ViewBag safely
        /// Usage in Razor: @Html.GetViewBag("Title")
        /// </summary>
        public static string GetViewBag(this IHtmlHelper htmlHelper, string key)
        {
            dynamic viewBag = htmlHelper.ViewBag;
            try
            {
                var value = ((IDictionary<string, object>)viewBag)[key];
                return value?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// Generate a CSS class based on condition
        /// Usage in Razor: @Html.CssClass(Model.IsActive, "active", "inactive")
        /// </summary>
        public static string CssClass(this IHtmlHelper htmlHelper, bool condition, string trueClass, string falseClass = "")
        {
            return condition ? trueClass : falseClass;
        }
        
        /// <summary>
        /// Generate multiple CSS classes based on conditions
        /// Usage in Razor: @Html.CssClasses(("active", Model.IsActive), ("disabled", Model.IsDisabled))
        /// </summary>
        public static string CssClasses(this IHtmlHelper htmlHelper, params (string className, bool condition)[] classes)
        {
            return string.Join(" ", classes.Where(c => c.condition).Select(c => c.className));
        }
    }
    
    /// <summary>
    /// Extension methods for ClaimsPrincipal to use directly in Razor views
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsSuperAdmin(this ClaimsPrincipal user)
        {
            return user?.HasClaim("IsSuperAdmin", "true") ?? false;
        }
        
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            if (user == null) return false;
            return user.IsInRole("Admin") || 
                   user.IsInRole("Administrator") || 
                   user.HasClaim("IsSuperAdmin", "true");
        }
        
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }
        
        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user?.FindFirst("UserName")?.Value ?? user?.Identity?.Name ?? "";
        }
        
        public static string GetFullName(this ClaimsPrincipal user)
        {
            return user?.FindFirst("FullName")?.Value ?? "";
        }
        
        public static string GetAvatarUrl(this ClaimsPrincipal user)
        {
            return user?.FindFirst("AvatarUrl")?.Value ?? "/images/default-avatar.png";
        }
        
        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }
    }
}