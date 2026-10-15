using KeyBasedAuthenticationTemplate_Contracts.Notifications;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IEmailService
    {
        Task<SendEmailResponse> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
    }
}
