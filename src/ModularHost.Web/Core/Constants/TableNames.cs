namespace MRCMS.Core.Constants
{
    public static class TableNames
    {
        // Core tables
        public const string Users = "Users";
        public const string Roles = "Roles";
        public const string UserRoles = "UserRoles";
        public const string UserClaims = "UserClaims";
        public const string UserLogins = "UserLogins";
        public const string UserTokens = "UserTokens";
        public const string RoleClaims = "RoleClaims";
        
        // Application tables
        public const string Menus = "Menus";
        public const string RolePermissions = "RolePermissions";
        public const string Logs = "Logs";
        public const string LogArchives = "LogArchives";
        public const string Notifications = "Notifications";
        public const string AuditLogs = "AuditLogs";
        public const string Settings = "Settings";
    }
}