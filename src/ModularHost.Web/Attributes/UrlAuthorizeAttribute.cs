using Microsoft.AspNetCore.Mvc;
using ModularHost.Web.Core.Enums;
using System;

namespace ModularHost.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UrlAuthorizeAttribute : Attribute
    {
        public AuthorizationPolicyType Policy { get; set; } = AuthorizationPolicyType.Authorize;
        public string Url { get; set; }
        public string HttpMethod { get; set; }
    }
}