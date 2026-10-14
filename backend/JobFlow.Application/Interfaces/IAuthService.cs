using JobFlow.Application.DTOs.Auth;

namespace JobFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task LogoutAsync(string refreshToken, string ipAddress);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
}
