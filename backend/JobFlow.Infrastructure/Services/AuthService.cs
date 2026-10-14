using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobFlow.Application.DTOs.Auth;
using JobFlow.Application.Interfaces;
using JobFlow.Domain.Entities;
using JobFlow.Domain.Enums;
using JobFlow.Infrastructure.Persistence;

namespace JobFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ITokenService tokenService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            throw new InvalidOperationException("Email is already registered.");

        if (request.Password != request.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role) || role == UserRole.Admin)
            throw new InvalidOperationException("Invalid role specified.");

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            Role = role
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, role.ToString());

        if (role == UserRole.JobSeeker)
        {
            _context.CandidateProfiles.Add(new CandidateProfile { UserId = user.Id });
            await _context.SaveChangesAsync();
        }

        return await CreateAuthResponseAsync(user, ipAddress);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account has been deactivated.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await CreateAuthResponseAsync(user, ipAddress);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Token == refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!token.IsActive)
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

        var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshToken.Token;

        newRefreshToken.UserId = token.UserId;
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(token.User);
        var accessToken = _tokenService.GenerateAccessToken(token.User, roles);

        return new AuthResponse(
            accessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt,
            MapToUserDto(token.User, roles)
        );
    }

    public async Task LogoutAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .SingleOrDefaultAsync(r => r.Token == refreshToken);

        if (token != null && token.IsActive)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");
        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserDto(user, roles);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user, string ipAddress)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

        refreshToken.UserId = user.Id;
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse(
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            MapToUserDto(user, roles)
        );
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.FirstName, user.LastName, user.Email!, roles.FirstOrDefault() ?? "", user.AvatarUrl, user.IsActive);
}
