using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    // Bound from the "Automation" section - in practice from Automation__* environment
    // variables loaded out of env/.env. The ApiKey is a secret and must never be logged.
    public class AutomationOptions
    {
        public const string SectionName = "Automation";

        [Required, Url]
        public string ApiBaseUrl { get; set; } = string.Empty;

        [Required]
        public string ApiKey { get; set; } = string.Empty;

        [Range(1, 600)]
        public int ApiTimeoutSeconds { get; set; } = 60;

        [Required]
        public string ReportOutputDirectory { get; set; } = "reports-output";

        [Required]
        public string LogDirectory { get; set; } = "logs";

        [Required, MinLength(1)]
        public string[] ReportRecipients { get; set; } = [];

        [Required, MinLength(1)]
        public string[] ErrorNotificationRecipients { get; set; } = [];
    }
}
