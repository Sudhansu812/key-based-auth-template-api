using System.Globalization;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    // Reflection-based DataAnnotations validation doesn't descend into nested option objects,
    // so the rules are written out here; every problem is reported at once.
    public class AutomationOptionsValidator : IValidateOptions<AutomationOptions>
    {
        public ValidateOptionsResult Validate(string? name, AutomationOptions options)
        {
            var errors = new List<string>();
            var api = options.Api;
            var reports = options.Reports;

            if (!Uri.TryCreate(api.BaseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme is not ("http" or "https"))
                errors.Add("Automation:Api:BaseUrl must be an absolute http(s) URL.");
            if (string.IsNullOrWhiteSpace(api.Key))
                errors.Add("Automation:Api:Key is required (set Automation__Api__Key in env/.env).");
            if (api.TimeoutSeconds is < 1 or > 600)
                errors.Add("Automation:Api:TimeoutSeconds must be between 1 and 600.");
            if (api.ErrorDetailMaxLength < 1)
                errors.Add("Automation:Api:ErrorDetailMaxLength must be at least 1.");
            Require(errors, api.Endpoints.Email, "Automation:Api:Endpoints:Email");
            Require(errors, api.Endpoints.ProductRiskStaging, "Automation:Api:Endpoints:ProductRiskStaging");

            Require(errors, reports.OutputDirectory, "Automation:Reports:OutputDirectory");
            Require(errors, reports.EmailSubject, "Automation:Reports:EmailSubject");
            RequireRecipients(errors, reports.Recipients, "Automation:Reports:Recipients");
            if (string.IsNullOrWhiteSpace(reports.FileTimestampFormat))
            {
                errors.Add("Automation:Reports:FileTimestampFormat is required.");
            }
            else
            {
                try { _ = DateTime.UtcNow.ToString(reports.FileTimestampFormat, CultureInfo.InvariantCulture); }
                catch (FormatException) { errors.Add("Automation:Reports:FileTimestampFormat is not a valid date format."); }
            }

            if (reports.Definitions.Count == 0)
                errors.Add("Automation:Reports:Definitions must define at least one report.");
            foreach (var (reportName, definition) in reports.Definitions)
            {
                Require(errors, definition.Endpoint, $"Automation:Reports:Definitions:{reportName}:Endpoint");
                Require(errors, definition.SheetName, $"Automation:Reports:Definitions:{reportName}:SheetName");
            }

            Require(errors, options.ErrorNotification.Subject, "Automation:ErrorNotification:Subject");
            RequireRecipients(errors, options.ErrorNotification.Recipients, "Automation:ErrorNotification:Recipients");

            return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
        }

        private static void Require(List<string> errors, string value, string path)
        {
            if (string.IsNullOrWhiteSpace(value)) errors.Add($"{path} is required.");
        }

        private static void RequireRecipients(List<string> errors, string[] recipients, string path)
        {
            if (recipients.Length == 0 || recipients.Any(string.IsNullOrWhiteSpace))
                errors.Add($"{path} needs at least one non-empty address.");
        }
    }
}
