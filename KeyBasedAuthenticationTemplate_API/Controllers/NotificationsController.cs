using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    // Mock email delivery. Sending is a side effect, so on top of ReportAccess it also requires
    // WriteAccess - a ReadOnly API key can read reports but cannot trigger emails.
    [ApiController]
    [Route("api/notifications")]
    [Authorize(Policy = ReportAccessRequirement.PolicyName)]
    public class NotificationsController : ControllerBase
    {
        // Attachments travel base64-encoded in the JSON body.
        private const long MaxRequestBodyBytes = 15 * 1024 * 1024;

        private readonly IEmailService _emailService;

        public NotificationsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("email")]
        [Authorize(Policy = "WriteAccess")]
        [RequestSizeLimit(MaxRequestBodyBytes)]
        public async Task<ActionResult<SendEmailResponse>> SendEmail(SendEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _emailService.SendAsync(request, cancellationToken);
            return Accepted(result);
        }
    }
}
