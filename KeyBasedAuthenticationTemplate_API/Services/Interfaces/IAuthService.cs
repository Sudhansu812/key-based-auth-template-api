using KeyBasedAuthenticationTemplate_API.DTOs.Auth;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
