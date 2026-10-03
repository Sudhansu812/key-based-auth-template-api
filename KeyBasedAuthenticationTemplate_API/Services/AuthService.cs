using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.DTOs.Auth;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher<User> passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (await _userRepository.ExistsByEmailOrUsernameAsync(request.Email, request.Username, cancellationToken))
            {
                throw new ConflictException("A user with that email or username already exists.");
            }

            var defaultRole = await _roleRepository.GetByNameAsync(SystemRoleNames.User, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"The '{SystemRoleNames.User}' system role is not seeded. Ensure DbInitializer has run.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = request.Username,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                Roles = new List<Role> { defaultRole }
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);
            if (user is null || !user.IsActive)
            {
                throw new UnauthorizedAppException("Invalid credentials.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAppException("Invalid credentials.");
            }

            return BuildAuthResponse(user);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);
            return new AuthResponse
            {
                AccessToken = token,
                ExpiresAtUtc = expiresAtUtc,
                UserId = user.Id,
                Username = user.Username,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };
        }
    }
}
