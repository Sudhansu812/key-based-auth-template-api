namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    public class ErrorNotificationOptions
    {
        public string[] Recipients { get; set; } = [];

        // string.Format template; {0} is the number of errors.
        public string Subject { get; set; } = string.Empty;
    }
}
