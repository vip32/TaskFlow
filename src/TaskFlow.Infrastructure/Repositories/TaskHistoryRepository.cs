using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<TaskHistoryRepository> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskHistoryRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="factory">DbContext factory.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription context accessor.</param>
    public TaskHistoryRepository(ILogger<TaskHistoryRepository> logger, IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            this.logger.LogDebug("TaskHistory - RegisterUsage (Repository): skipping registration for blank name");
            return;
        }

        var normalized = name.Trim();
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogDebug("TaskHistory - RegisterUsage (Repository): registering usage for subscription {SubscriptionId}. IsSubTaskName={IsSubTaskName}, NameLength={NameLength}", subscriptionId, isSubTaskName, normalized.Length);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var existing = await db.TaskHistories
            .FirstOrDefaultAsync(
                h => h.SubscriptionId == subscriptionId
                    && h.IsSubTaskName == isSubTaskName
                    && h.Name.ToLower() == normalized.ToLower(),
                cancellationToken);

        if (existing is null)
        {
            this.logger.LogInformation("TaskHistory - RegisterUsage (Repository): creating new entry for subscription {SubscriptionId}. IsSubTaskName={IsSubTaskName}", subscriptionId, isSubTaskName);
            db.TaskHistories.Add(new TaskHistory(subscriptionId, normalized, isSubTaskName));
        }
        else
        {
            this.logger.LogDebug("TaskHistory - RegisterUsage (Repository): incrementing usage for existing entry {TaskHistoryId}", existing.Id);
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
        this.logger.LogDebug("TaskHistory - GetSuggestions (Repository): getting suggestions for subscription {SubscriptionId}. IsSubTaskName={IsSubTaskName}, PrefixLength={PrefixLength}, Take={Take}", subscriptionId, isSubTaskName, normalizedPrefix.Length, take);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var query = db.TaskHistories
            .AsNoTracking()
            .Where(h => h.SubscriptionId == subscriptionId && h.IsSubTaskName == isSubTaskName);

        if (!string.IsNullOrEmpty(normalizedPrefix))
        {
            query = query.Where(h => EF.Functions.Like(h.Name.ToLower(), $"{normalizedPrefix}%"));
        }

        var suggestions = await query
            .OrderByDescending(h => h.UsageCount)
            .ThenByDescending(h => h.LastUsedAt)
            .ThenBy(h => h.Name)
            .Select(h => h.Name)
            .Take(take)
            .ToListAsync(cancellationToken);
        this.logger.LogDebug("TaskHistory - GetSuggestions (Repository): resolved {SuggestionCount} suggestion(s)", suggestions.Count);
        return suggestions;
    }
}
