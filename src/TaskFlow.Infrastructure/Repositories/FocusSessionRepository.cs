using Microsoft.EntityFrameworkCore;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusSessionRepository"/> class.
    /// </summary>
    /// <param name="factory">DbContext factory.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public FocusSessionRepository(IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<List<FocusSession>> GetRecentAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var maxTake = take <= 0 ? 20 : take;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.FocusSessions
            .AsNoTracking()
            .Where(session => session.SubscriptionId == subscriptionId)
            .OrderByDescending(session => session.StartedAt)
            .Take(maxTake)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<FocusSession> GetRunningAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.FocusSessions
            .FirstOrDefaultAsync(session => session.SubscriptionId == subscriptionId && session.EndedAt == DateTime.MinValue, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<FocusSession> AddAsync(FocusSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.FocusSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }

    /// <inheritdoc/>
    public async Task<FocusSession> UpdateAsync(FocusSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.FocusSessions.Update(session);
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }
}
