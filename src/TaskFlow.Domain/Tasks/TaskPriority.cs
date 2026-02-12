namespace TaskFlow.Domain;

/// <summary>
/// Defines the relative urgency of a task.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low urgency task.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal urgency task.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High urgency task.
    /// </summary>
    High = 3,
}
