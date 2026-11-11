namespace KeyBasedAuthenticationTemplate_API.Authorization
{
    // Role-based policy names (see AuthenticationServiceExtensions). Admin satisfies all of them.
    public static class Policies
    {
        // Reader, Writer, Reporter or Admin.
        public const string ReadAccess = "ReadAccess";

        // Writer or Admin.
        public const string WriteAccess = "WriteAccess";

        // Reporter or Admin. Report data spans every owner, so it has its own role.
        public const string ReportAccess = "ReportAccess";
    }
}
