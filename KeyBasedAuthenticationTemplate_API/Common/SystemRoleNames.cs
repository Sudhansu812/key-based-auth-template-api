namespace KeyBasedAuthenticationTemplate_API.Common
{
    // Reserved role names used by the authorization policies and DbInitializer seeding.
    // These must match the "IsSystemRole" rows exactly. An API key can hold any combination.
    public static class SystemRoleNames
    {
        // Manages API keys and roles, sees every owner's products, and passes every policy.
        public const string Admin = "Admin";

        // GET products (own owner's only).
        public const string Reader = "Reader";

        // Create/update/delete products (own owner's only), send notifications, load staging.
        public const string Writer = "Writer";

        // Report endpoints, notifications and staging (the Automation job's role). Also reads
        // every owner's products, since GET /api/products doubles as a report.
        public const string Reporter = "Reporter";
    }
}
