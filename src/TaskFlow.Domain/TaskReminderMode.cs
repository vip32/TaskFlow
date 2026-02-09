namespace TaskFlow.Domain;

/// <summary>
/// Describes how a task reminder trigger is calculated.
/// </summary>
public enum TaskReminderMode
{
    /// <summary>
    /// Reminder is calculated relative to the task due date and time.
    /// </summary>
    RelativeToDueDateTime = 0,

    /// <summary>
    /// Reminder uses an explicit local time on the task due date.
    /// </summary>
    DateOnlyFallbackTime = 1,
}
