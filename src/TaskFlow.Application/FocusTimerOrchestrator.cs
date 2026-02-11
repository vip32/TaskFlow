using Microsoft.Extensions.Logging;
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
    private readonly ILogger<FocusTimerOrchestrator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusTimerOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="focusSessionRepository">Focus session repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public FocusTimerOrchestrator(ILogger<FocusTimerOrchestrator> logger, IFocusSessionRepository focusSessionRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.focusSessionRepository = focusSessionRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> StartAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("FocusSession - Start: starting focus session. TaskId={TaskId}", taskId);
        var running = await this.focusSessionRepository.GetRunningAsync(cancellationToken);
        if (running is not null)
        {
            this.logger.LogInformation("FocusSession - Start: ending currently running focus session {SessionId} before starting a new one", running.Id);
            running.End();
            await this.focusSessionRepository.UpdateAsync(running, cancellationToken);
        }

        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var session = taskId == Guid.Empty
            ? new FocusSession(subscriptionId)
            : new FocusSession(subscriptionId, taskId);
        var created = await this.focusSessionRepository.AddAsync(session, cancellationToken);
        this.logger.LogInformation("FocusSession - Start: started focus session {SessionId} for subscription {SubscriptionId}", created.Id, subscriptionId);
        return created;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> EndCurrentAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("FocusSession - EndCurrent: ending current focus session");
        var running = await this.focusSessionRepository.GetRunningAsync(cancellationToken);
        if (running is null)
        {
            this.logger.LogDebug("FocusSession - EndCurrent: no running focus session found");
            return null;
        }

        running.End();
        var ended = await this.focusSessionRepository.UpdateAsync(running, cancellationToken);
        this.logger.LogInformation("FocusSession - EndCurrent: ended focus session {SessionId}", ended.Id);
        return ended;
    }

    /// <inheritdoc/>
    public Task<List<FocusSession>> GetRecentAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("FocusSession - GetRecent: fetching recent focus sessions. Take={Take}", take);
        return this.focusSessionRepository.GetRecentAsync(take, cancellationToken);
    }
}
