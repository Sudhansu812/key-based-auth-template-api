namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    public class ApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        // Secret - never put it in appsettings.json.
        public string Key { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; } = 60;

        // API error bodies are truncated to this many characters before they go into messages/logs.
        public int ErrorDetailMaxLength { get; set; } = 300;

        public ApiEndpointsOptions Endpoints { get; set; } = new();
    }

    // Paths are relative to BaseUrl. Report endpoints live with their report definition instead.
    public class ApiEndpointsOptions
    {
        public string Email { get; set; } = string.Empty;
        public string ProductRiskStaging { get; set; } = string.Empty;
    }
}
