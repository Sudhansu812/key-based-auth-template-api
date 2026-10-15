using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Notifications;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    // Stand-in for a real mail provider: validates the message and logs it, nothing is sent.
    // Replace the registration in ApplicationServiceExtensions with an SMTP/provider-backed
    // IEmailService to deliver real mail.
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task<SendEmailResponse> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
        {
            // Attachment bodies are decoded only to prove they are valid base64 and to report
            // their size - they are never written to the log.
            var attachmentSummaries = new List<string>(request.Attachments.Count);
            foreach (var attachment in request.Attachments)
            {
                var buffer = new byte[(attachment.ContentBase64.Length * 3 / 4) + 3];
                if (!Convert.TryFromBase64String(attachment.ContentBase64, buffer, out var bytesWritten))
                {
                    throw new BadRequestException($"Attachment '{attachment.FileName}' is not valid base64.");
                }

                attachmentSummaries.Add($"{attachment.FileName} ({bytesWritten} bytes)");
            }

            var response = new SendEmailResponse
            {
                MessageId = Guid.NewGuid(),
                AcceptedAtUtc = DateTime.UtcNow
            };

            _logger.LogInformation(
                "[MOCK EMAIL] {MessageId} to {Recipients} | subject: {Subject} | attachments: {Attachments}",
                response.MessageId,
                request.To,
                request.Subject,
                attachmentSummaries);

            return Task.FromResult(response);
        }
    }
}
