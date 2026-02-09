namespace TaskFlow.Domain;

/// <summary>
/// Represents one focus timer session and its optional task association.
/// </summary>
public class FocusSession
{
    private FocusSession()
    {
    }

    /// <summary>
    /// Gets the subscription identifier that owns this focus session.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the focus session.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the associated task identifier.
    /// Guid.Empty indicates no task is linked.
    /// </summary>
    public Guid TaskId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the session started.
    /// </summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the session ended.
    /// DateTime.MinValue indicates the session is still running.
    /// </summary>
    public DateTime EndedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the session has ended.
    /// </summary>
    public bool IsCompleted => this.EndedAt != DateTime.MinValue;

    /// <summary>
    /// Gets the elapsed duration of the session.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            var endTime = DateTime.UtcNow;
            if (this.IsCompleted)
            {
                endTime = this.EndedAt;
            }

            return endTime - this.StartedAt;
        }
    }

    /// <summary>
    /// Initializes a new focus session without task association.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    public FocusSession(Guid subscriptionId)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        this.SubscriptionId = subscriptionId;
        this.Id = Guid.NewGuid();
        this.TaskId = Guid.Empty;
        this.StartedAt = DateTime.UtcNow;
        this.EndedAt = DateTime.MinValue;
    }

    /// <summary>
    /// Initializes a new focus session with optional task association.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="taskId">Task identifier to associate. Guid.Empty leaves it unassigned.</param>
    public FocusSession(Guid subscriptionId, Guid taskId)
        : this(subscriptionId)
    {
        if (taskId != Guid.Empty)
        {
            this.TaskId = taskId;
        }
    }

    /// <summary>
    /// Ends the focus session.
    /// </summary>
    public void End()
    {
        if (this.IsCompleted)
        {
            return;
        }

        this.EndedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Associates the session with a task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    public void AttachToTask(Guid taskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(taskId));
        }

        this.TaskId = taskId;
    }
}
