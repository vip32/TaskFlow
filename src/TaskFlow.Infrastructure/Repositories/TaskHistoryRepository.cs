using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for task history suggestions.
/// </summary>
[RegisterScoped(ServiceType = typeof(ITaskHistoryRepository))]
public sealed class TaskHistoryRepository : ITaskHistoryRepository
{
    private readonly IDbContextFactory<AppDbContext> factory;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskHistoryRepository"/> class.
    /// </summary>
    /// <param name="factory">DbContext factory.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription context accessor.</param>
    public TaskHistoryRepository(IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var normalized = name.Trim();
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var existing = await db.TaskHistories
            .FirstOrDefaultAsync(
                h => h.SubscriptionId == subscriptionId
                    && h.IsSubTaskName == isSubTaskName
                    && h.Name.ToLower() == normalized.ToLower(),
                cancellationToken);

        if (existing is null)
        {
            db.TaskHistories.Add(new TaskHistory(subscriptionId, normalized, isSubTaskName));
        }
        else
        {
            existing.MarkUsed();
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : prefix.Trim().ToLowerInvariant();

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var query = db.TaskHistories
            .AsNoTracking()
            .Where(h => h.SubscriptionId == subscriptionId && h.IsSubTaskName == isSubTaskName);

        if (!string.IsNullOrEmpty(normalizedPrefix))
        {
            query = query.Where(h => EF.Functions.Like(h.Name.ToLower(), $"{normalizedPrefix}%"));
        }

        return await query
            .OrderByDescending(h => h.UsageCount)
            .ThenByDescending(h => h.LastUsedAt)
            .ThenBy(h => h.Name)
            .Select(h => h.Name)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
