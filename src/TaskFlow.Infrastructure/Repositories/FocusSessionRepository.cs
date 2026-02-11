using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for focus session persistence.
/// </summary>
[RegisterScoped(ServiceType = typeof(IFocusSessionRepository))]
public sealed class FocusSessionRepository : IFocusSessionRepository
{
    private readonly IDbContextFactory<AppDbContext> factory;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;
    private readonly ILogger<FocusSessionRepository> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusSessionRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="factory">DbContext factory.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public FocusSessionRepository(ILogger<FocusSessionRepository> logger, IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<List<FocusSession>> GetRecentAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var maxTake = take <= 0 ? 20 : take;
        this.logger.LogDebug("FocusSession - GetRecent (Repository): getting recent sessions for subscription {SubscriptionId}. RequestedTake={RequestedTake}, EffectiveTake={EffectiveTake}", subscriptionId, take, maxTake);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        var sessions = await db.FocusSessions
            .AsNoTracking()
            .Where(session => session.SubscriptionId == subscriptionId)
            .OrderByDescending(session => session.StartedAt)
            .Take(maxTake)
            .ToListAsync(cancellationToken);
        this.logger.LogDebug("FocusSession - GetRecent (Repository): retrieved {SessionCount} recent session(s)", sessions.Count);
        return sessions;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> GetRunningAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogDebug("FocusSession - GetRunning (Repository): getting running session for subscription {SubscriptionId}", subscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        var session = await db.FocusSessions
            .FirstOrDefaultAsync(session => session.SubscriptionId == subscriptionId && session.EndedAt == DateTime.MinValue, cancellationToken);
        if (session is null)
        {
            this.logger.LogDebug("FocusSession - GetRunning (Repository): no running session found for subscription {SubscriptionId}", subscriptionId);
        }

        return session;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> AddAsync(FocusSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        this.logger.LogInformation("FocusSession - Add (Repository): adding session {SessionId} for subscription {SubscriptionId}", session.Id, session.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.FocusSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> UpdateAsync(FocusSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        this.logger.LogInformation("FocusSession - Update (Repository): updating session {SessionId} for subscription {SubscriptionId}. IsCompleted={IsCompleted}", session.Id, session.SubscriptionId, session.IsCompleted);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.FocusSessions.Update(session);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }
}
