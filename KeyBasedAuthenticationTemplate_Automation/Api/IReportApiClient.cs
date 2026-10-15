using KeyBasedAuthenticationTemplate_Contracts.Notifications;
using KeyBasedAuthenticationTemplate_Contracts.Staging;

namespace KeyBasedAuthenticationTemplate_Automation.Api
{
    public interface IReportApiClient
    {
        Task<List<T>> GetListAsync<T>(string path, CancellationToken cancellationToken = default);
        Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
        Task<StagingLoadResponse> LoadProductRiskStagingAsync(CancellationToken cancellationToken = default);
    }
}
