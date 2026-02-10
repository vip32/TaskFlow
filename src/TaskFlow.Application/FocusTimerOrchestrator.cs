using Injectio.Attributes;
using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Default focus timer orchestrator.
/// </summary>
[RegisterScoped(ServiceType = typeof(IFocusTimerOrchestrator))]
public sealed class FocusTimerOrchestrator : IFocusTimerOrchestrator
{
    private readonly IFocusSessionRepository focusSessionRepository;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusTimerOrchestrator"/> class.
    /// </summary>
    /// <param name="focusSessionRepository">Focus session repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public FocusTimerOrchestrator(IFocusSessionRepository focusSessionRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.focusSessionRepository = focusSessionRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> StartAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var running = await this.focusSessionRepository.GetRunningAsync(cancellationToken);
        if (running is not null)
        {
            running.End();
            await this.focusSessionRepository.UpdateAsync(running, cancellationToken);
        }

        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var session = taskId == Guid.Empty
            ? new FocusSession(subscriptionId)
            : new FocusSession(subscriptionId, taskId);
        return await this.focusSessionRepository.AddAsync(session, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<FocusSession> EndCurrentAsync(CancellationToken cancellationToken = default)
    {
        var running = await this.focusSessionRepository.GetRunningAsync(cancellationToken);
        if (running is null)
        {
            return null;
        }

        running.End();
        return await this.focusSessionRepository.UpdateAsync(running, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<FocusSession>> GetRecentAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        return this.focusSessionRepository.GetRecentAsync(take, cancellationToken);
    }
}
