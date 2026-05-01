using System.Text.Json;
using AI.TaskFlow.Application.Common;
using AI.TaskFlow.Application.DTOs;
using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Domain.Entities;
using AI.TaskFlow.Domain.Enums;
using AI.TaskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.TaskFlow.Infrastructure.Services;

public sealed class TaskService : ITaskService
{
    private readonly IAuditLogService _auditLogService;
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext, IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResponse<TaskDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNumber = Math.Max(pageNumber, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);

        var query = _dbContext.TaskItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => ToTaskDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResponse<TaskDto>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<IReadOnlyCollection<TaskDto>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskItems
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToTaskDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskItems
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToTaskDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Task title is required.");
        }

        var projectExists = await _dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new KeyNotFoundException("Project was not found.");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var assignedUserExists = await _dbContext.Users.AnyAsync(x => x.Id == request.AssignedToUserId.Value, cancellationToken);
            if (!assignedUserExists)
            {
                throw new KeyNotFoundException("Assigned user was not found.");
            }
        }

        var taskItem = new TaskItem
        {
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Status = TaskItemStatus.Todo,
            DueDate = request.DueDate
        };

        _dbContext.TaskItems.Add(taskItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = request.AssignedToUserId,
            Action = "CreateTask",
            EntityName = nameof(TaskItem),
            EntityId = taskItem.Id.ToString(),
            NewValues = JsonSerializer.Serialize(new { taskItem.Title, taskItem.ProjectId, taskItem.AssignedToUserId })
        }, cancellationToken);

        return await GetByIdAsync(taskItem.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load created task.");
    }

    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var taskItem = await _dbContext.TaskItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (taskItem is null)
        {
            return null;
        }

        if (request.AssignedToUserId.HasValue)
        {
            var assignedUserExists = await _dbContext.Users.AnyAsync(x => x.Id == request.AssignedToUserId.Value, cancellationToken);
            if (!assignedUserExists)
            {
                throw new KeyNotFoundException("Assigned user was not found.");
            }
        }

        var oldValues = JsonSerializer.Serialize(new
        {
            taskItem.Title,
            taskItem.Description,
            taskItem.AssignedToUserId,
            taskItem.Priority,
            taskItem.Status,
            taskItem.DueDate
        });

        taskItem.Title = string.IsNullOrWhiteSpace(request.Title) ? taskItem.Title : request.Title.Trim();
        taskItem.Description = request.Description?.Trim();
        taskItem.AssignedToUserId = request.AssignedToUserId;
        taskItem.Priority = request.Priority;
        taskItem.Status = request.Status;
        taskItem.DueDate = request.DueDate;
        taskItem.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = taskItem.AssignedToUserId,
            Action = "UpdateTask",
            EntityName = nameof(TaskItem),
            EntityId = taskItem.Id.ToString(),
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(new
            {
                taskItem.Title,
                taskItem.Description,
                taskItem.AssignedToUserId,
                taskItem.Priority,
                taskItem.Status,
                taskItem.DueDate
            })
        }, cancellationToken);

        return await GetByIdAsync(taskItem.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var taskItem = await _dbContext.TaskItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (taskItem is null)
        {
            return false;
        }

        taskItem.IsDeleted = true;
        taskItem.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAsync(new AuditLog
        {
            UserId = taskItem.AssignedToUserId,
            Action = "DeleteTask",
            EntityName = nameof(TaskItem),
            EntityId = taskItem.Id.ToString(),
            NewValues = "Task soft deleted."
        }, cancellationToken);

        return true;
    }

    private static TaskDto ToTaskDto(TaskItem taskItem)
    {
        return new TaskDto
        {
            Id = taskItem.Id,
            ProjectId = taskItem.ProjectId,
            AssignedToUserId = taskItem.AssignedToUserId,
            Title = taskItem.Title,
            Description = taskItem.Description,
            Priority = taskItem.Priority,
            Status = taskItem.Status,
            DueDate = taskItem.DueDate
        };
    }
}
