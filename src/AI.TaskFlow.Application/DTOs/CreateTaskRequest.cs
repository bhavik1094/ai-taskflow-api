using AI.TaskFlow.Domain.Enums;

namespace AI.TaskFlow.Application.DTOs;

public sealed class CreateTaskRequest
{
    public Guid ProjectId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
}
