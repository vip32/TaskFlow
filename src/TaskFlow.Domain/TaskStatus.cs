namespace TaskFlow.Domain;

/// <summary>
/// Describes the workflow state of a task.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is newly created and not started.
    /// </summary>
    New = 0,

    /// <summary>
    /// Task is actively being worked on.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task is paused and intentionally not active.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// Task is completed.
    /// </summary>
    Done = 3,

    /// <summary>
    /// Task is cancelled.
    /// </summary>
    Cancelled = 4,
}
