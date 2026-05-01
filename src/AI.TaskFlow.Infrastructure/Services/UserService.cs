using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.TaskFlow.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Where(x => x.Id == id)
            .Select(x => new UserProfileDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Status = x.Status,
                Roles = x.UserRoles.Select(ur => ur.Role.Name).ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<UserProfileDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(pageNumber, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);

        var query = _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new UserProfileDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Status = x.Status,
                Roles = x.UserRoles.Select(ur => ur.Role.Name).ToArray()
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<UserProfileDto>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords
        };
    }
}
