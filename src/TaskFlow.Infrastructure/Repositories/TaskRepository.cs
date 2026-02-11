using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for task aggregate persistence.
/// </summary>
[RegisterScoped(ServiceType = typeof(ITaskRepository))]
public sealed class TaskRepository : ITaskRepository
{
    private readonly IDbContextFactory<AppDbContext> factory;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskRepository"/> class.
    /// </summary>
    /// <param name="factory">DbContext factory used to create per-call contexts.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription context accessor.</param>
    public TaskRepository(IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.ProjectId == projectId && !t.ParentTaskId.HasValue)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.ParentTaskId == parentTaskId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> GetNextSortOrderAsync(Guid? projectId, Guid? parentTaskId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        var maxSortOrder = await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.SubscriptionId == subscriptionId &&
                t.ProjectId == projectId &&
                t.ParentTaskId == parentTaskId)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync(cancellationToken);

        return maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0;
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.ProjectId == projectId && t.Priority == priority)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalized = query.Trim().ToLowerInvariant();

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.ProjectId == projectId)
            .Where(t =>
                EF.Functions.Like(t.Title.ToLower(), $"%{normalized}%") ||
                EF.Functions.Like((t.Note ?? string.Empty).ToLower(), $"%{normalized}%"))
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.ProjectId == projectId && t.IsFocused)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var minCreatedAt = DateTime.UtcNow.AddDays(-Math.Abs(days));

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.CreatedAt >= minCreatedAt)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var minCreatedAt = DateTime.UtcNow.AddDays(-Math.Abs(days));

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && !t.ProjectId.HasValue && t.CreatedAt >= minCreatedAt)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.DueDateLocal.HasValue && t.DueDateLocal == localDate)
            .OrderBy(t => t.DueTimeLocal)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.SubscriptionId == subscriptionId &&
                t.DueDateLocal.HasValue &&
                t.DueDateLocal >= localStartInclusive &&
                t.DueDateLocal <= localEndInclusive)
            .OrderBy(t => t.DueDateLocal)
            .ThenBy(t => t.DueTimeLocal)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && t.DueDateLocal.HasValue && t.DueDateLocal > localDateExclusive)
            .OrderBy(t => t.DueDateLocal)
            .ThenBy(t => t.DueTimeLocal)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskIds);

        var ids = taskIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.SubscriptionId == subscriptionId && ids.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> AddAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        EnsureSubscriptionMatch(task.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        EnsureSubscriptionMatch(task.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Tasks.Update(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> UpdateRangeAsync(IEnumerable<DomainTask> tasks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var list = tasks.ToList();
        foreach (var task in list)
        {
            EnsureSubscriptionMatch(task.SubscriptionId);
        }

        if (list.Count == 0)
        {
            return [];
        }

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Tasks.UpdateRange(list);
        await db.SaveChangesAsync(cancellationToken);
        return list;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var task = await db.Tasks.FirstOrDefaultAsync(t => t.SubscriptionId == subscriptionId && t.Id == id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var task = await db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.SubscriptionId == subscriptionId && t.Id == id, cancellationToken);

        if (task is null)
        {
            throw new KeyNotFoundException($"Task with id '{id}' was not found.");
        }

        return task;
    }

    private void EnsureSubscriptionMatch(Guid entitySubscriptionId)
    {
        var currentSubscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        if (entitySubscriptionId != currentSubscriptionId)
        {
            throw new InvalidOperationException("Task subscription does not match current subscription context.");
        }
    }

}
