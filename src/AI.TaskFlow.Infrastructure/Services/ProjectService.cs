using System.Text.Json;
using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Domain.Entities;
using AI.TaskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.TaskFlow.Infrastructure.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IAuditLogService _auditLogService;
    private readonly AppDbContext _dbContext;

    public ProjectService(AppDbContext dbContext, IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResponse<ProjectDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(pageNumber, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);

        var query = _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Owner)
            .Include(x => x.Tasks)
            .OrderByDescending(x => x.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => ToProjectDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResponse<ProjectDto>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Owner)
            .Include(x => x.Tasks)
            .Where(x => x.Id == id)
            .Select(x => ToProjectDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Project name is required.");
        }

        var owner = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == request.OwnerId, cancellationToken);

        if (owner is null)
        {
            throw new KeyNotFoundException("Project owner was not found.");
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            OwnerId = request.OwnerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedBy = owner.Email
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = owner.Id,
            Action = "CreateProject",
            EntityName = nameof(Project),
            EntityId = project.Id.ToString(),
            NewValues = JsonSerializer.Serialize(new { project.Name, project.Description, project.OwnerId })
        }, cancellationToken);

        return await GetByIdAsync(project.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load created project.");
    }

    public async Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var oldValues = JsonSerializer.Serialize(new { project.Name, project.Description, project.StartDate, project.EndDate });

        project.Name = string.IsNullOrWhiteSpace(request.Name) ? project.Name : request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = project.OwnerId,
            Action = "UpdateProject",
            EntityName = nameof(Project),
            EntityId = project.Id.ToString(),
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(new { project.Name, project.Description, project.StartDate, project.EndDate })
        }, cancellationToken);

        return await GetByIdAsync(project.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
        {
            return false;
        }

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = project.OwnerId,
            Action = "DeleteProject",
            EntityName = nameof(Project),
            EntityId = project.Id.ToString(),
            NewValues = "Project soft deleted."
        }, cancellationToken);

        return true;
    }

    private static ProjectDto ToProjectDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner is null ? null : $"{project.Owner.FirstName} {project.Owner.LastName}".Trim(),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            TaskCount = project.Tasks.Count
        };
    }
}
