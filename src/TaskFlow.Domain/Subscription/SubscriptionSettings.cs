namespace TaskFlow.Domain;

/// <summary>
/// Stores configurable subscription-scoped preferences.
/// </summary>
public class SubscriptionSettings
{
    /// <summary>
    /// Gets the owning subscription identifier.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether completed tasks are always shown in list and board views.
    /// </summary>
    public bool AlwaysShowCompletedTasks { get; private set; }

    private SubscriptionSettings()
    {
    }

    /// <summary>
    /// Initializes subscription settings for a subscription.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="alwaysShowCompletedTasks">Initial completed-task visibility preference.</param>
    public SubscriptionSettings(Guid subscriptionId, bool alwaysShowCompletedTasks = false)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        this.SubscriptionId = subscriptionId;
        this.AlwaysShowCompletedTasks = alwaysShowCompletedTasks;
    }

    /// <summary>
    /// Updates completed-task visibility behavior.
    /// </summary>
    /// <param name="alwaysShowCompletedTasks">Target completed-task visibility preference.</param>
    public void SetAlwaysShowCompletedTasks(bool alwaysShowCompletedTasks)
    {
        this.AlwaysShowCompletedTasks = alwaysShowCompletedTasks;
    }
}
