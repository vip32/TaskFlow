using Microsoft.Extensions.Logging;
using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Default application orchestrator for subscription settings use cases.
/// </summary>
[RegisterScoped(ServiceType = typeof(ISubscriptionSettingsOrchestrator))]
public sealed class SubscriptionSettingsOrchestrator : ISubscriptionSettingsOrchestrator
{
    private readonly ILogger<SubscriptionSettingsOrchestrator> logger;
    private readonly ISubscriptionRepository subscriptionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionSettingsOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="subscriptionRepository">Subscription repository.</param>
    public SubscriptionSettingsOrchestrator(
        ILogger<SubscriptionSettingsOrchestrator> logger,
        ISubscriptionRepository subscriptionRepository)
    {
        this.logger = logger;
        this.subscriptionRepository = subscriptionRepository;
    }

    /// <inheritdoc />
    public Task<SubscriptionSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("SubscriptionSettings - Get: fetching settings for current subscription");
        return GetSettingsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SubscriptionSettings> UpdateAlwaysShowCompletedTasksAsync(bool alwaysShowCompletedTasks, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("SubscriptionSettings - UpdateAlwaysShowCompletedTasks: updating completed-task visibility to {AlwaysShowCompletedTasks}", alwaysShowCompletedTasks);

        var subscription = await this.subscriptionRepository.GetCurrentAsync(cancellationToken);
        subscription.SetAlwaysShowCompletedTasks(alwaysShowCompletedTasks);
        var updated = await this.subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        return updated.Settings;
    }

    private async Task<SubscriptionSettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var subscription = await this.subscriptionRepository.GetCurrentAsync(cancellationToken);
        return subscription.Settings;
    }
}
