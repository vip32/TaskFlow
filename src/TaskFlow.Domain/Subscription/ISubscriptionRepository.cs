namespace TaskFlow.Domain;

/// <summary>
/// Provides persistence operations for <see cref="Subscription"/> aggregates.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Gets the current subscription aggregate.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The current subscription aggregate.</returns>
    Task<Subscription> GetCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the current subscription aggregate.
    /// </summary>
    /// <param name="subscription">Subscription aggregate to persist.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated subscription aggregate.</returns>
    Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
