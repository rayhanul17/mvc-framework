using Microsoft.AspNetCore.Mvc;
using MRCMS.Core.Enums;
using System;

namespace MRCMS.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UrlAuthorizeAttribute : Attribute
    {
        public AuthorizationPolicyType Policy { get; set; } = AuthorizationPolicyType.Authorize;
        public string Url { get; set; }
        public string HttpMethod { get; set; }
    }
}