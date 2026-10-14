using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobFlow.Application.DTOs.Auth;
using JobFlow.Application.Interfaces;

namespace JobFlow.Api.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request, ClientIpAddress);
        return CreatedResponse(result, "Registration successful");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, ClientIpAddress);
        return OkResponse(result, "Login successful");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ClientIpAddress);
        return OkResponse(result, "Token refreshed");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken, ClientIpAddress);
        return OkResponse<object>(null!, "Logged out successfully");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _authService.GetCurrentUserAsync(CurrentUserId);
        return OkResponse(user);
    }
}
