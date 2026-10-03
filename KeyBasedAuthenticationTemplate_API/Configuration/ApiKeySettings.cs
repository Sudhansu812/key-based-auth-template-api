namespace KeyBasedAuthenticationTemplate_API.Configuration
{
    public class ApiKeySettings
    {
        public const string SectionName = "ApiKey";

        public string Prefix { get; set; } = "kbat";
    }
}
