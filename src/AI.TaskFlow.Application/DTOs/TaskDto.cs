using AI.TaskFlow.Domain.Enums;

namespace AI.TaskFlow.Application.DTOs;

public sealed class TaskDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
}
