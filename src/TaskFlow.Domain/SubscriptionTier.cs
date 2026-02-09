namespace TaskFlow.Domain;

/// <summary>
/// Defines the commercial tier of a subscription.
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Free tier with the most limits.
    /// </summary>
    Free = 0,

    /// <summary>
    /// Plus tier with reduced limits.
    /// </summary>
    Plus = 1,

    /// <summary>
    /// Pro tier with full feature access.
    /// </summary>
    Pro = 2,
}
