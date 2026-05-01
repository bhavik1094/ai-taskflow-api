using System.Security.Cryptography;
using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Domain.Entities;
using AI.TaskFlow.Domain.Enums;
using AI.TaskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.TaskFlow.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private const string DefaultRoleName = "User";

    private readonly IAuditLogService _auditLogService;
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(AppDbContext dbContext, IJwtTokenService jwtTokenService, IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _auditLogService = auditLogService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegisterRequest(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var role = await GetOrCreateDefaultRoleAsync(cancellationToken);

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Status = UserStatus.Active,
            CreatedBy = normalizedEmail
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = role
        });

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = _jwtTokenService.GetRefreshTokenExpiry(),
            CreatedBy = normalizedEmail
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "Register",
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            NewValues = $"User {user.Email} registered."
        }, cancellationToken);

        return BuildAuthResponse(user, new[] { role.Name }, refreshTokenValue);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenAccessException($"User account is {user.Status}.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = normalizedEmail;

        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = _jwtTokenService.GetRefreshTokenExpiry(),
            CreatedBy = normalizedEmail
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = user.Id,
            Action = "Login",
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            NewValues = $"User {user.Email} logged in."
        }, cancellationToken);

        return BuildAuthResponse(user, user.UserRoles.Select(x => x.Role.Name), refreshTokenValue);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenEntity = await _dbContext.RefreshTokens
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (tokenEntity is null || tokenEntity.IsDeleted)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        if (tokenEntity.RevokedAt.HasValue || tokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.ReplacedByToken = newRefreshToken;
        tokenEntity.UpdatedAt = DateTime.UtcNow;
        tokenEntity.UpdatedBy = tokenEntity.User.Email;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = tokenEntity.UserId,
            Token = newRefreshToken,
            ExpiresAt = _jwtTokenService.GetRefreshTokenExpiry(),
            CreatedBy = tokenEntity.User.Email
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = tokenEntity.UserId,
            Action = "RefreshToken",
            EntityName = nameof(RefreshToken),
            EntityId = tokenEntity.Id.ToString(),
            NewValues = "Refresh token rotated."
        }, cancellationToken);

        return BuildAuthResponse(tokenEntity.User, tokenEntity.User.UserRoles.Select(x => x.Role.Name), newRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenEntity = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (tokenEntity is null)
        {
            throw new KeyNotFoundException("Refresh token was not found.");
        }

        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.UpdatedAt = DateTime.UtcNow;
        tokenEntity.IsDeleted = true;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = tokenEntity.UserId,
            Action = "RevokeRefreshToken",
            EntityName = nameof(RefreshToken),
            EntityId = tokenEntity.Id.ToString(),
            NewValues = "Refresh token revoked."
        }, cancellationToken);
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("First name, last name, email, and password are required.");
        }
    }

    private AuthResponse BuildAuthResponse(User user, IEnumerable<string> roles, string refreshToken)
    {
        var roleNames = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        return new AuthResponse
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user, roleNames),
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Status = user.Status,
                Roles = roleNames
            }
        };
    }

    private async Task<Role> GetOrCreateDefaultRoleAsync(CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == DefaultRoleName, cancellationToken);
        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            Name = DefaultRoleName,
            Description = "Default application user role.",
            CreatedBy = "system"
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    private static class PasswordHasher
    {
        private const int Iterations = 100_000;
        private const int KeySize = 32;
        private const int SaltSize = 16;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
