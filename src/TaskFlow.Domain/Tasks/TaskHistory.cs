namespace TaskFlow.Domain;

/// <summary>
/// Stores historical task and subtask titles for autocomplete suggestions.
/// </summary>
public class TaskHistory
{
    /// <summary>
    /// Gets the unique identifier of the history entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the subscription identifier that owns this history entry.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets the normalized title used for suggestions.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the name was used for a subtask.
    /// </summary>
    public bool IsSubTaskName { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this name was last used.
    /// </summary>
    public DateTime LastUsedAt { get; private set; }

    /// <summary>
    /// Gets how many times this name was used.
    /// </summary>
    public int UsageCount { get; private set; }

    private TaskHistory()
    {
        this.Name = string.Empty;
    }

    /// <summary>
    /// Initializes a new task name history entry.
    /// </summary>
    /// <param name="subscriptionId">Subscription identifier.</param>
    /// <param name="name">Task or subtask name.</param>
    /// <param name="isSubTaskName">Whether the name belongs to a subtask context.</param>
    public TaskHistory(Guid subscriptionId, string name, bool isSubTaskName)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Task history name cannot be empty.", nameof(name));
        }

        this.Id = Guid.NewGuid();
        this.SubscriptionId = subscriptionId;
        this.Name = name.Trim();
        this.IsSubTaskName = isSubTaskName;
        this.LastUsedAt = DateTime.UtcNow;
        this.UsageCount = 1;
    }

    /// <summary>
    /// Marks the history name as used again.
    /// </summary>
    public void MarkUsed()
    {
        this.LastUsedAt = DateTime.UtcNow;
        this.UsageCount++;
    }
}
