using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_Contracts.Notifications
{
    public class SendEmailRequest : IValidatableObject
    {
        public const int MaxRecipients = 50;
        public const int MaxAttachments = 10;

        [Required, MinLength(1), MaxLength(MaxRecipients)]
        public List<string> To { get; set; } = new();

        [Required, MaxLength(256)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(20000)]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; }

        [MaxLength(MaxAttachments)]
        public List<EmailAttachment> Attachments { get; set; } = new();

        // [EmailAddress] on the list would not reach individual items, so check them here.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailValidator = new EmailAddressAttribute();
            foreach (var address in To)
            {
                if (!emailValidator.IsValid(address))
                {
                    yield return new ValidationResult($"'{address}' is not a valid email address.", [nameof(To)]);
                }
            }
        }
    }
}
