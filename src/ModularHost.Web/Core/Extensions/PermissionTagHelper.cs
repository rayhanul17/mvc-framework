using Microsoft.AspNetCore.Razor.TagHelpers;
using MRCMS.Core.Extensions;

namespace MRCMS.Core.Extensions
{
    [HtmlTargetElement("*", Attributes = "asp-permission")]
    [HtmlTargetElement("*", Attributes = "asp-role")]
    [HtmlTargetElement("*", Attributes = "asp-auth-only")]
    [HtmlTargetElement("*", Attributes = "asp-admin-only")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IPermissionHelper _permissionHelper;

        public PermissionTagHelper(IPermissionHelper permissionHelper)
        {
            _permissionHelper = permissionHelper;
        }

        [HtmlAttributeName("asp-permission")]
        public string Permission { get; set; } = string.Empty;

        [HtmlAttributeName("asp-role")]
        public string Role { get; set; } = string.Empty;

        [HtmlAttributeName("asp-auth-only")]
        public bool AuthOnly { get; set; }

        [HtmlAttributeName("asp-admin-only")]
        public bool AdminOnly { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            bool shouldShow = true;

            if (AuthOnly && !_permissionHelper.IsAuthenticated())
            {
                shouldShow = false;
            }

            if (AdminOnly && !_permissionHelper.IsAdmin())
            {
                shouldShow = false;
            }

            if (!string.IsNullOrEmpty(Permission) && !_permissionHelper.HasPermission(Permission))
            {
                shouldShow = false;
            }

            if (!string.IsNullOrEmpty(Role) && !_permissionHelper.IsInRole(Role))
            {
                shouldShow = false;
            }

            if (!shouldShow)
            {
                output.SuppressOutput();
            }
        }
    }
}