using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Success(response, "Registration completed successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Success(response, "Login completed successfully."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromQuery] string refreshToken, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Success(response, "Token refreshed successfully."));
    }

    [HttpPost("revoke")]
    public async Task<ActionResult<ApiResponse<object>>> Revoke([FromQuery] string refreshToken, CancellationToken cancellationToken)
    {
        await _authService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        return Ok(ApiResponse<object>.Success(null, "Refresh token revoked successfully."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                          User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserProfileDto>.Failure("User is not authenticated.", "Missing or invalid user id claim."));
        }

        var response = await _userService.GetByIdAsync(userId, cancellationToken);
        if (response is null)
        {
            return NotFound(ApiResponse<UserProfileDto>.Failure("User profile was not found.", $"No user exists for id '{userId}'."));
        }

        return Ok(ApiResponse<UserProfileDto>.Success(response, "Current user profile loaded successfully."));
    }
}
