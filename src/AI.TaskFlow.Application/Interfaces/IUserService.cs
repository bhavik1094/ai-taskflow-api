using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;

namespace AI.TaskFlow.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserProfileDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
