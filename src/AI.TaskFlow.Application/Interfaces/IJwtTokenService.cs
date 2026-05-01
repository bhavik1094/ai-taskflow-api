using AI.TaskFlow.Domain.Entities;

namespace AI.TaskFlow.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}
