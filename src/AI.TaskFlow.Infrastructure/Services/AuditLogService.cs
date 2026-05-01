using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Domain.Entities;
using AI.TaskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.TaskFlow.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _dbContext;

    public AuditLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<AuditLog>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(pageNumber, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);

        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLog>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        auditLog.CreatedAt = DateTime.UtcNow;
        auditLog.CreatedBy ??= "system";

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
