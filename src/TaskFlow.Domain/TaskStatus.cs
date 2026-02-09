namespace TaskFlow.Domain;

/// <summary>
/// Describes the workflow state of a task.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is not started.
    /// </summary>
    ToDo = 0,

    /// <summary>
    /// Task is actively being worked on.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task is completed.
    /// </summary>
    Done = 2,
}
