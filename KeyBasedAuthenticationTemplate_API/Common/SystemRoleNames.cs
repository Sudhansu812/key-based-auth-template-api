namespace KeyBasedAuthenticationTemplate_API.Common
{
    // Reserved role names used by [Authorize(Roles = ...)] and DbInitializer seeding.
    // These must match the "IsSystemRole" rows exactly.
    public static class SystemRoleNames
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
