using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for subscription aggregate persistence.
/// </summary>
[RegisterScoped(ServiceType = typeof(ISubscriptionRepository))]
public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ILogger<SubscriptionRepository> logger;
    private readonly IDbContextFactory<AppDbContext> factory;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="factory">DbContext factory used to create per-call contexts.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription context accessor.</param>
    public SubscriptionRepository(
        ILogger<SubscriptionRepository> logger,
        IDbContextFactory<AppDbContext> factory,
        ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc />
    public async Task<Subscription> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogDebug("Subscription - GetCurrent (Repository): getting aggregate for subscription {SubscriptionId}", subscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        var subscription = await db.Subscriptions
            .Include(candidate => candidate.Settings)
            .FirstOrDefaultAsync(candidate => candidate.Id == subscriptionId, cancellationToken);

        if (subscription is null)
        {
            this.logger.LogWarning("Subscription - GetCurrent (Repository): subscription {SubscriptionId} not found", subscriptionId);
            throw new EntityNotFoundException(nameof(Subscription), subscriptionId);
        }

        if (subscription.Settings is null)
        {
            this.logger.LogInformation("Subscription - GetCurrent (Repository): settings missing for subscription {SubscriptionId}, creating defaults.", subscriptionId);
            subscription.SetAlwaysShowCompletedTasks(false);
            await db.SaveChangesAsync(cancellationToken);
        }

        return subscription;
    }

    /// <inheritdoc />
    public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        EnsureSubscriptionMatch(subscription.Id);

        this.logger.LogInformation("Subscription - Update (Repository): updating subscription {SubscriptionId}", subscription.Id);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Subscriptions.Update(subscription);
        await db.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    private void EnsureSubscriptionMatch(Guid entitySubscriptionId)
    {
        var currentSubscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        if (entitySubscriptionId != currentSubscriptionId)
        {
            this.logger.LogWarning("Subscription - EnsureSubscriptionMatch (Repository): subscription mismatch. EntitySubscriptionId={EntitySubscriptionId}, CurrentSubscriptionId={CurrentSubscriptionId}", entitySubscriptionId, currentSubscriptionId);
            throw new InvalidOperationException("Subscription does not match current subscription context.");
        }
    }
}
