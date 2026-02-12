using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Coordinates subscription settings use cases.
/// </summary>
public interface ISubscriptionSettingsOrchestrator
{
    /// <summary>
    /// Gets settings for the current subscription.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Current subscription settings.</returns>
    Task<SubscriptionSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the completed-task visibility preference for the current subscription.
    /// </summary>
    /// <param name="alwaysShowCompletedTasks">Whether completed tasks should always be shown.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Updated subscription settings.</returns>
    Task<SubscriptionSettings> UpdateAlwaysShowCompletedTasksAsync(bool alwaysShowCompletedTasks, CancellationToken cancellationToken = default);
}
