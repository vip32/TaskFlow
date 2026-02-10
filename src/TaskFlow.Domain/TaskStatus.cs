namespace TaskFlow.Domain;

/// <summary>
/// Describes the workflow state of a task.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is in the backlog and not started yet.
    /// </summary>
    Todo = 0,

    /// <summary>
    /// Task is actively being worked on.
    /// </summary>
    Doing = 1,

    /// <summary>
    /// Task is done.
    /// </summary>
    Done = 3,
}
