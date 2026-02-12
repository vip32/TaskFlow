using TaskFlow.Domain;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.Application;

/// <summary>
/// Serializable task payload for import/export workflows.
/// </summary>
public sealed record TaskExportDto
{
    /// <summary>
    /// Gets task identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets parent task identifier for subtasks.
    /// </summary>
    public Guid? ParentTaskId { get; init; }

    /// <summary>
    /// Gets task title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets optional task note.
    /// </summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>
    /// Gets priority.
    /// </summary>
    public TaskPriority Priority { get; init; }

    /// <summary>
    /// Gets workflow status.
    /// </summary>
    public DomainTaskStatus Status { get; init; }

    /// <summary>
    /// Gets completion flag.
    /// </summary>
    public bool IsCompleted { get; init; }

    /// <summary>
    /// Gets focus flag.
    /// </summary>
    public bool IsFocused { get; init; }

    /// <summary>
    /// Gets explicit today marker flag.
    /// </summary>
    public bool IsMarkedForToday { get; init; }

    /// <summary>
    /// Gets creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets completion timestamp (UTC).
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Gets sort order.
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Gets local due date.
    /// </summary>
    public DateOnly? DueDateLocal { get; init; }

    /// <summary>
    /// Gets local due time.
    /// </summary>
    public TimeOnly? DueTimeLocal { get; init; }

    /// <summary>
    /// Gets due instant in UTC.
    /// </summary>
    public DateTime? DueAtUtc { get; init; }

    /// <summary>
    /// Gets task tags.
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets nested subtasks.
    /// </summary>
    public List<TaskExportDto> SubTasks { get; init; } = [];
}
