namespace KeyBasedAuthenticationTemplate_Contracts.Authentication
{
    // The request header API keys are presented in - shared so server and clients can't drift apart.
    public static class ApiKeyHeader
    {
        public const string Name = "X-Api-Key";
    }
}
