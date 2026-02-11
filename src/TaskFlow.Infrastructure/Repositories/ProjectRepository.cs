using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for project aggregate persistence.
/// </summary>
[RegisterScoped(ServiceType = typeof(IProjectRepository))]
public sealed class ProjectRepository : IProjectRepository
{
    private readonly IDbContextFactory<AppDbContext> factory;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
    /// </summary>
    /// <param name="factory">DbContext factory used to create per-call contexts.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription context accessor.</param>
    public ProjectRepository(IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        return await db.Projects
            .AsNoTracking()
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId && p.Id == id, cancellationToken);

        if (project is null)
        {
            throw new KeyNotFoundException($"Project with id '{id}' was not found.");
        }

        return project;
    }

    /// <inheritdoc/>
    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        EnsureSubscriptionMatch(project.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        EnsureSubscriptionMatch(project.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Projects.Update(project);
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId && p.Id == id, cancellationToken);
        if (project is null)
        {
            return false;
        }

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void EnsureSubscriptionMatch(Guid entitySubscriptionId)
    {
        var currentSubscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        if (entitySubscriptionId != currentSubscriptionId)
        {
            throw new InvalidOperationException("Project subscription does not match current subscription context.");
        }
    }

}
