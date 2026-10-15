using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_Contracts.Notifications
{
    public class EmailAttachment
    {
        [Required, MaxLength(256)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(128)]
        public string ContentType { get; set; } = "application/octet-stream";

        // Base64-encoded file content.
        [Required]
        public string ContentBase64 { get; set; } = string.Empty;
    }
}
