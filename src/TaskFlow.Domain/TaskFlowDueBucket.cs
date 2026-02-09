namespace TaskFlow.Domain;

/// <summary>
/// Defines due-date bucket filters for My Task Flow sections.
/// </summary>
public enum TaskFlowDueBucket
{
    /// <summary>
    /// No due-date filter.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Includes tasks due today (or explicitly marked for today).
    /// </summary>
    Today = 1,

    /// <summary>
    /// Includes tasks due later in the current week.
    /// </summary>
    ThisWeek = 2,

    /// <summary>
    /// Includes tasks due after the current week.
    /// </summary>
    Upcoming = 3,

    /// <summary>
    /// Includes recently created tasks.
    /// </summary>
    Recent = 4,

    /// <summary>
    /// Includes tasks without due date.
    /// </summary>
    NoDueDate = 5,
}
