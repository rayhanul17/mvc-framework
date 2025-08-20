namespace Nexora.Core.Constants;

/// <summary>
/// Centralized role constants to avoid hardcoded role names throughout the application
/// </summary>
public static class RoleConstants
{
    // System Roles
    public const string SuperAdmin = "SuperAdmin";
    public const string Administrator = "Administrator";
    public const string Admin = "Admin"; // Alias for Administrator
    public const string User = "User";
    
    // Customer Support Roles
    public const string CustomerSupportAdmin = "CustomerSupportAdmin";
    public const string CustomerSupportManager = "CustomerSupportManager";
    public const string CustomerSupportAgent = "CustomerSupportAgent";
    public const string CustomerSupportCustomer = "CustomerSupportCustomer";
    
    // Combined role checks
    public static readonly string[] AdminRoles = { SuperAdmin, Administrator, Admin };
    public static readonly string[] SupportRoles = { CustomerSupportAdmin, CustomerSupportManager, CustomerSupportAgent };
    public static readonly string[] AllSupportRoles = { CustomerSupportAdmin, CustomerSupportManager, CustomerSupportAgent, CustomerSupportCustomer };
    
    /// <summary>
    /// Check if user has any admin role
    /// </summary>
    public static bool IsAdmin(params string[] userRoles)
    {
        return userRoles.Any(role => AdminRoles.Contains(role));
    }
    
    /// <summary>
    /// Check if user has any support role
    /// </summary>
    public static bool IsSupport(params string[] userRoles)
    {
        return userRoles.Any(role => SupportRoles.Contains(role));
    }
}