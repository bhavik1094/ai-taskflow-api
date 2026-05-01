using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;

namespace AI.TaskFlow.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResponse<TaskDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaskDto>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
