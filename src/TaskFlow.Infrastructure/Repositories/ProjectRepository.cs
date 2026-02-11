using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ProjectRepository> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="factory">DbContext factory used to create per-call contexts.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription context accessor.</param>
    public ProjectRepository(ILogger<ProjectRepository> logger, IDbContextFactory<AppDbContext> factory, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.factory = factory;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogDebug("Project - GetAll (Repository): getting all projects for subscription {SubscriptionId}", subscriptionId);

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
        this.logger.LogDebug("Project - GetById (Repository): getting project {ProjectId} for subscription {SubscriptionId}", id, subscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId && p.Id == id, cancellationToken);

        if (project is null)
        {
            this.logger.LogWarning("Project - GetById (Repository): project {ProjectId} not found for subscription {SubscriptionId}", id, subscriptionId);
            throw new EntityNotFoundException(nameof(Project), id);
        }

        return project;
    }

    /// <inheritdoc/>
    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        EnsureSubscriptionMatch(project.SubscriptionId);
        this.logger.LogInformation("Project - Add (Repository): adding project {ProjectId} for subscription {SubscriptionId}", project.Id, project.SubscriptionId);

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
        this.logger.LogInformation("Project - Update (Repository): updating project {ProjectId} for subscription {SubscriptionId}", project.Id, project.SubscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);
        db.Projects.Update(project);
        await db.SaveChangesAsync(cancellationToken);
        return project;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogInformation("Project - Delete (Repository): deleting project {ProjectId} for subscription {SubscriptionId}", id, subscriptionId);

        await using var db = await this.factory.CreateDbContextAsync(cancellationToken);

        var project = await db.Projects.FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId && p.Id == id, cancellationToken);
        if (project is null)
        {
            this.logger.LogWarning("Project - Delete (Repository): project {ProjectId} was not found for deletion in subscription {SubscriptionId}", id, subscriptionId);
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
            this.logger.LogWarning("Project - EnsureSubscriptionMatch (Repository): subscription mismatch. EntitySubscriptionId={EntitySubscriptionId}, CurrentSubscriptionId={CurrentSubscriptionId}", entitySubscriptionId, currentSubscriptionId);
            throw new InvalidOperationException("Project subscription does not match current subscription context.");
        }
    }

}
