namespace TaskFlow.Domain;

/// <summary>
/// Represents a reminder attached to a task.
/// </summary>
public class TaskReminder
{
    /// <summary>
    /// Gets the unique reminder identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the task identifier this reminder belongs to.
    /// </summary>
    public Guid TaskId { get; private set; }

    /// <summary>
    /// Gets the reminder mode.
    /// </summary>
    public TaskReminderMode Mode { get; private set; }

    /// <summary>
    /// Gets minutes before due date-time for relative reminders.
    /// </summary>
    public int MinutesBefore { get; private set; }

    /// <summary>
    /// Gets fallback local time used for date-only reminders.
    /// </summary>
    public TimeOnly FallbackLocalTime { get; private set; }

    /// <summary>
    /// Gets the UTC instant when reminder should be delivered.
    /// </summary>
    public DateTime TriggerAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC instant when reminder was delivered.
    /// </summary>
    public DateTime? SentAtUtc { get; private set; }

    private TaskReminder()
    {
    }

    private TaskReminder(Guid taskId, TaskReminderMode mode, int minutesBefore, TimeOnly fallbackLocalTime, DateTime triggerAtUtc)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(taskId));
        }

        this.Id = Guid.NewGuid();
        this.TaskId = taskId;
        this.Mode = mode;
        this.MinutesBefore = minutesBefore;
        this.FallbackLocalTime = fallbackLocalTime;
        this.TriggerAtUtc = triggerAtUtc;
        this.SentAtUtc = null;
    }

    /// <summary>
    /// Creates a relative reminder for a due date-time task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="minutesBefore">Minutes before due time.</param>
    /// <param name="dueAtUtc">Task due instant in UTC.</param>
    /// <returns>Created reminder.</returns>
    public static TaskReminder CreateRelative(Guid taskId, int minutesBefore, DateTime? dueAtUtc)
    {
        if (!dueAtUtc.HasValue)
        {
            throw new InvalidOperationException("Relative reminder requires a due date and time.");
        }

        if (minutesBefore < 0)
        {
            throw new ArgumentException("Minutes before cannot be negative.", nameof(minutesBefore));
        }

        var triggerAtUtc = dueAtUtc.Value.AddMinutes(-minutesBefore);
        return new TaskReminder(taskId, TaskReminderMode.RelativeToDueDateTime, minutesBefore, TimeOnly.MinValue, triggerAtUtc);
    }

    /// <summary>
    /// Creates a reminder for a date-only task using fallback local time.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="dueDateLocal">Due date in local subscription timezone.</param>
    /// <param name="fallbackLocalTime">Fallback local time for trigger.</param>
    /// <param name="timeZone">Subscription timezone.</param>
    /// <returns>Created reminder.</returns>
    public static TaskReminder CreateDateOnlyFallback(Guid taskId, DateOnly dueDateLocal, TimeOnly fallbackLocalTime, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        if (dueDateLocal == DateOnly.MinValue)
        {
            throw new InvalidOperationException("Date-only reminder requires a due date.");
        }

        var localDateTime = dueDateLocal.ToDateTime(fallbackLocalTime, DateTimeKind.Unspecified);
        var triggerAtUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);

        return new TaskReminder(taskId, TaskReminderMode.DateOnlyFallbackTime, 0, fallbackLocalTime, triggerAtUtc);
    }

    /// <summary>
    /// Marks reminder as delivered.
    /// </summary>
    /// <param name="sentAtUtc">Delivery time in UTC.</param>
    public void MarkSent(DateTime sentAtUtc)
    {
        if (sentAtUtc == DateTime.MinValue)
        {
            throw new ArgumentException("Sent timestamp must be a valid UTC instant.", nameof(sentAtUtc));
        }

        if (this.SentAtUtc.HasValue)
        {
            return;
        }

        this.SentAtUtc = sentAtUtc;
    }
}
