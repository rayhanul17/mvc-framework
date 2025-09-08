namespace MRCMS.Core.Enums
{
    public enum AccessType
    {
        Anonymous = 0,      // Anyone can access, no authentication required
        Authenticated = 1,  // Must be logged in
        Authorized = 2      // Must be logged in and have specific role permission
    }
}