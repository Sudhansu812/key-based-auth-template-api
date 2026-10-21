namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    // Bound from the "Automation" section of appsettings.json. Secrets (Api:Key) are not stored there:
    // they come from env/.env or real environment variables, e.g. Automation__Api__Key. Any value can be
    // overridden the same way (Automation__Reports__OutputDirectory, ...). Logging is configured by the
    // standard "Serilog" section in the same file.
    public class AutomationOptions
    {
        public const string SectionName = "Automation";

        public ApiOptions Api { get; set; } = new();
        public ReportsOptions Reports { get; set; } = new();
        public ErrorNotificationOptions ErrorNotification { get; set; } = new();
    }
}
