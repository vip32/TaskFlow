namespace TaskFlow.Domain;

/// <summary>
/// Provides access to the current subscription context.
/// </summary>
public interface ICurrentSubscriptionAccessor
{
    /// <summary>
    /// Gets the current subscription.
    /// </summary>
    /// <returns>The current subscription instance.</returns>
    Subscription GetCurrentSubscription();
}
