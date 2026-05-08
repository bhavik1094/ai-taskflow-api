using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AI.TaskFlow.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    private static readonly string[] UserIdClaimTypes =
    [
        ClaimTypes.NameIdentifier,
        JwtRegisteredClaimNames.Sub,
        "sub",
        "userId"
    ];

    public static bool TryGetAuthenticatedUserId(this ClaimsPrincipal? principal, out Guid userId)
    {
        userId = Guid.Empty;

        if (principal is null)
        {
            return false;
        }

        foreach (var claimType in UserIdClaimTypes)
        {
            var claimValue = principal.FindFirstValue(claimType);
            if (Guid.TryParse(claimValue, out userId))
            {
                return true;
            }
        }

        return false;
    }
}
