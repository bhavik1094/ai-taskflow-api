using AI.TaskFlow.Domain.Common;
using AI.TaskFlow.Domain.Enums;

namespace AI.TaskFlow.Domain.Entities;

public sealed class TaskItem : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public DateTime? DueDate { get; set; }

    public Project Project { get; set; } = null!;
    public User? AssignedToUser { get; set; }
}
