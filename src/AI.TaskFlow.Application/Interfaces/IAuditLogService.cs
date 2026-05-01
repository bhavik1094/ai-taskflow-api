using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Domain.Entities;

namespace AI.TaskFlow.Application.Interfaces;

public interface IAuditLogService
{
    Task<PagedResponse<AuditLog>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
